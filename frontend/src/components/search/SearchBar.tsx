interface SearchBarProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  isSearching?: boolean;
}

export default function SearchBar({ value, onChange, placeholder = 'Search templates...', isSearching = false }: SearchBarProps) {
  return (
    <div className="search-bar" style={{ position: 'relative', width: '100%' }}>
      <input
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        style={{
          width: '100%',
          padding: '0.75rem 2.5rem 0.75rem 1rem',
          fontSize: '1rem',
          border: '1px solid #444',
          borderRadius: '8px',
          background: '#1a1a1a',
          color: 'rgba(255, 255, 255, 0.87)',
        }}
      />
      {isSearching && (
        <div
          style={{
            position: 'absolute',
            right: '1rem',
            top: '50%',
            transform: 'translateY(-50%)',
            color: '#646cff',
          }}
        >
          <span>Searching...</span>
        </div>
      )}
      {!isSearching && value && (
        <button
          onClick={() => onChange('')}
          style={{
            position: 'absolute',
            right: '1rem',
            top: '50%',
            transform: 'translateY(-50%)',
            background: 'transparent',
            border: 'none',
            color: '#888',
            cursor: 'pointer',
            fontSize: '1.5rem',
            padding: 0,
          }}
          aria-label="Clear search"
        >
          ×
        </button>
      )}
    </div>
  );
}
