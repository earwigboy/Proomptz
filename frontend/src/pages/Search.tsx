import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useSearch } from '../lib/hooks/useSearch';
import SearchBar from '../components/search/SearchBar';
import SearchResults from '../components/search/SearchResults';

export default function Search() {
  const [query, setQuery] = useState('');
  const [page] = useState(1);
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const { results, isLoading, isSearching } = useSearch(query, page);

  return (
    <div style={{ maxWidth: '1400px', margin: '0 auto', padding: '2rem' }}>
      <div style={{ marginBottom: '2rem' }}>
        <button
          onClick={() => {
            const params = searchParams.toString();
            navigate(`/${params ? `?${params}` : ''}`);
          }}
          style={{
            marginBottom: '1rem',
            background: 'transparent',
            border: '1px solid #444',
            color: 'rgba(255, 255, 255, 0.87)',
            padding: '0.5rem 1rem',
            borderRadius: '4px',
            cursor: 'pointer',
          }}
          aria-label="Go back to templates list"
        >
          ← Back to Templates
        </button>
        <h1>Search Templates</h1>
      </div>

      <div style={{ marginBottom: '2rem' }}>
        <SearchBar
          value={query}
          onChange={setQuery}
          placeholder="Search templates by name or content..."
          isSearching={isSearching}
        />
      </div>

      <SearchResults results={results} isLoading={isLoading} query={query} />
    </div>
  );
}
