using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;
using PromptTemplateManager.Core.Entities;

namespace PromptTemplateManager.Infrastructure.FileSystem;

/// <summary>
/// Provides full-text search capabilities using Lucene.NET.
/// </summary>
public class FileSystemSearchEngine : IDisposable
{
    private static readonly LuceneVersion LuceneVersion = LuceneVersion.LUCENE_48;
    private readonly string _indexPath;
    private readonly ILogger<FileSystemSearchEngine> _logger;
    private readonly FSDirectory _directory;
    private readonly StandardAnalyzer _analyzer;
    private IndexWriter? _indexWriter;
    private SearcherManager? _searcherManager;

    public FileSystemSearchEngine(string indexPath, ILogger<FileSystemSearchEngine> logger)
    {
        _indexPath = indexPath;
        _logger = logger;

        // Ensure index directory exists
        if (!System.IO.Directory.Exists(_indexPath))
        {
            System.IO.Directory.CreateDirectory(_indexPath);
        }

        _directory = FSDirectory.Open(_indexPath);
        _analyzer = new StandardAnalyzer(LuceneVersion);

        InitializeIndex();
    }

    /// <summary>
    /// Initializes the Lucene index or opens existing index.
    /// </summary>
    private void InitializeIndex()
    {
        var indexConfig = new IndexWriterConfig(LuceneVersion, _analyzer)
        {
            OpenMode = OpenMode.CREATE_OR_APPEND
        };

        _indexWriter = new IndexWriter(_directory, indexConfig);
        _indexWriter.Commit();

        _searcherManager = new SearcherManager(_directory, null);
    }

    /// <summary>
    /// Indexes a template document.
    /// </summary>
    /// <param name="template">The template to index.</param>
    public async Task IndexTemplateAsync(Template template)
    {
        if (_indexWriter == null)
        {
            throw new InvalidOperationException("Index writer not initialized");
        }

        await Task.Run(() =>
        {
            var doc = new Document
            {
                // Stored fields (returned in search results)
                new StringField("id", template.Id.ToString(), Field.Store.YES),
                new StringField("name", template.Name, Field.Store.YES),
                new StringField("folderId", template.FolderId?.ToString() ?? string.Empty, Field.Store.YES),

                // Indexed fields (searchable, not stored)
                new TextField("nameSearchable", template.Name, Field.Store.NO),
                new TextField("content", template.Content, Field.Store.NO),

                // Date fields (for sorting/filtering)
                new StringField("created", template.CreatedAt.ToString("O"), Field.Store.YES),
                new StringField("updated", template.UpdatedAt.ToString("O"), Field.Store.YES)
            };

            // Remove old document if exists (update scenario)
            _indexWriter.UpdateDocument(new Term("id", template.Id.ToString()), doc);
            _indexWriter.Commit();

            _logger.LogDebug("Indexed template {TemplateId}: {TemplateName}", template.Id, template.Name);
        });

        // Refresh searcher to see latest changes
        _searcherManager?.MaybeRefreshBlocking();
    }

    /// <summary>
    /// Removes a template from the index.
    /// </summary>
    /// <param name="templateId">The ID of the template to remove.</param>
    public async Task RemoveTemplateAsync(Guid templateId)
    {
        if (_indexWriter == null)
        {
            throw new InvalidOperationException("Index writer not initialized");
        }

        await Task.Run(() =>
        {
            _indexWriter.DeleteDocuments(new Term("id", templateId.ToString()));
            _indexWriter.Commit();

            _logger.LogDebug("Removed template {TemplateId} from index", templateId);
        });

        _searcherManager?.MaybeRefreshBlocking();
    }

    /// <summary>
    /// Searches for templates matching the query with metadata-based ranking.
    /// Recently updated templates receive a boost in ranking.
    /// </summary>
    /// <param name="searchTerm">The search query.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <returns>List of template IDs ranked by relevance and recency.</returns>
    public async Task<List<Guid>> SearchAsync(string searchTerm, int maxResults = 100)
    {
        if (_searcherManager == null)
        {
            throw new InvalidOperationException("Search manager not initialized");
        }

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<Guid>();
        }

        return await Task.Run(() =>
        {
            var searcher = _searcherManager.Acquire();
            try
            {
                var queryParser = new MultiFieldQueryParser(
                    LuceneVersion,
                    new[] { "nameSearchable", "content" },
                    _analyzer
                );

                var query = queryParser.Parse(searchTerm);

                // Retrieve more results than needed to allow for recency-based re-ranking
                var hits = searcher.Search(query, maxResults * 2);

                // Apply metadata-based ranking: boost recently updated templates
                var scoredResults = new List<(Guid id, double finalScore)>();
                var now = DateTime.UtcNow;

                foreach (var scoreDoc in hits.ScoreDocs)
                {
                    var doc = searcher.Doc(scoreDoc.Doc);
                    var idString = doc.Get("id");
                    var updatedString = doc.Get("updated");

                    if (Guid.TryParse(idString, out var id))
                    {
                        double recencyBoost = 1.0;

                        // Calculate recency boost: templates updated in last 30 days get higher scores
                        if (DateTime.TryParse(updatedString, out var updatedAt))
                        {
                            var daysSinceUpdate = (now - updatedAt).TotalDays;

                            if (daysSinceUpdate <= 7)
                            {
                                recencyBoost = 1.5; // 50% boost for templates updated in last week
                            }
                            else if (daysSinceUpdate <= 30)
                            {
                                recencyBoost = 1.2; // 20% boost for templates updated in last month
                            }
                            else if (daysSinceUpdate <= 90)
                            {
                                recencyBoost = 1.1; // 10% boost for templates updated in last 3 months
                            }
                        }

                        // Combine Lucene relevance score with recency boost
                        var finalScore = scoreDoc.Score * recencyBoost;
                        scoredResults.Add((id, finalScore));
                    }
                }

                // Sort by final score (descending) and take top maxResults
                var results = scoredResults
                    .OrderByDescending(r => r.finalScore)
                    .Take(maxResults)
                    .Select(r => r.id)
                    .ToList();

                _logger.LogInformation("Search for '{SearchTerm}' returned {ResultCount} results with metadata-based ranking", searchTerm, results.Count);

                return results;
            }
            catch (ParseException ex)
            {
                _logger.LogWarning(ex, "Failed to parse search query: {SearchTerm}", searchTerm);
                return new List<Guid>();
            }
            finally
            {
                _searcherManager.Release(searcher);
            }
        });
    }

    /// <summary>
    /// Rebuilds the entire index from scratch.
    /// </summary>
    /// <param name="templates">All templates to index.</param>
    public async Task RebuildIndexAsync(IEnumerable<Template> templates)
    {
        if (_indexWriter == null)
        {
            throw new InvalidOperationException("Index writer not initialized");
        }

        await Task.Run(() =>
        {
            _indexWriter.DeleteAll();
            _indexWriter.Commit();

            _logger.LogInformation("Rebuilding search index...");

            foreach (var template in templates)
            {
                var doc = new Document
                {
                    new StringField("id", template.Id.ToString(), Field.Store.YES),
                    new StringField("name", template.Name, Field.Store.YES),
                    new StringField("folderId", template.FolderId?.ToString() ?? string.Empty, Field.Store.YES),
                    new TextField("nameSearchable", template.Name, Field.Store.NO),
                    new TextField("content", template.Content, Field.Store.NO),
                    new StringField("created", template.CreatedAt.ToString("O"), Field.Store.YES),
                    new StringField("updated", template.UpdatedAt.ToString("O"), Field.Store.YES)
                };

                _indexWriter.AddDocument(doc);
            }

            _indexWriter.Commit();
            _logger.LogInformation("Search index rebuilt successfully");
        });

        _searcherManager?.MaybeRefreshBlocking();
    }

    /// <summary>
    /// Disposes resources used by the search engine.
    /// </summary>
    public void Dispose()
    {
        _searcherManager?.Dispose();
        _indexWriter?.Dispose();
        _analyzer?.Dispose();
        _directory?.Dispose();
    }
}
