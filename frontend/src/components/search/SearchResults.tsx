import { useNavigate } from 'react-router-dom';
import type { TemplateSummary } from '../../lib/api-client';

interface SearchResultsProps {
  results: TemplateSummary[];
  isLoading: boolean;
  query: string;
}

export default function SearchResults({ results, isLoading, query }: SearchResultsProps) {
  const navigate = useNavigate();

  if (isLoading) {
    return <div className="loading">Searching...</div>;
  }

  if (!query) {
    return (
      <div style={{ textAlign: 'center', padding: '2rem', color: '#888' }}>
        <p>Enter a search query to find templates</p>
      </div>
    );
  }

  if (results.length === 0) {
    return (
      <div style={{ textAlign: 'center', padding: '2rem', color: '#888' }}>
        <p>No templates found for "{query}"</p>
      </div>
    );
  }

  return (
    <div className="search-results">
      <div style={{ marginBottom: '1rem', color: '#888' }} role="status" aria-live="polite">
        <p>Found {results.length} result{results.length !== 1 ? 's' : ''} for "{query}"</p>
      </div>

      <div className="templates-grid" style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(3, 1fr)',
        gap: '1.5rem'
      }} role="list" aria-label="Search results">
        {results.map((template) => (
          <div
            key={template.id}
            className="template-card"
            style={{
              background: '#1a1a1a',
              border: '1px solid #444',
              borderRadius: '8px',
              padding: '1.5rem',
              cursor: 'pointer',
            }}
            onClick={() => navigate(`/use/${template.id}`)}
            onKeyDown={(e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                navigate(`/use/${template.id}`);
              }
            }}
            tabIndex={0}
            role="listitem"
            aria-label={`Template: ${template.name}`}
          >
            <div className="template-header">
              <h3>{template.name}</h3>
              {template.folderName && (
                <div style={{ fontSize: '0.875rem', color: '#888', marginTop: '0.25rem' }}>
                  📁 {template.folderName}
                </div>
              )}
            </div>

            <div className="template-preview" style={{
              marginTop: '1rem',
              fontSize: '0.875rem',
              color: '#aaa',
              lineHeight: '1.5',
            }}>
              {template.contentPreview}
              {template.contentPreview && template.contentPreview.length >= 200 && '...'}
            </div>

            <div className="template-footer" style={{
              marginTop: '1rem',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
            }}>
              <div style={{ fontSize: '0.75rem', color: '#666' }}>
                Updated: {new Date(template.updatedAt || '').toLocaleDateString()}
              </div>
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/use/${template.id}`);
                }}
                className="btn-create"
                style={{ fontSize: '0.875rem', padding: '0.375rem 0.75rem' }}
                aria-label={`Use template ${template.name}`}
              >
                Use Template
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
