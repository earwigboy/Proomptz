import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Search, X } from 'lucide-react';

interface SearchBarProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  isSearching?: boolean;
}

export default function SearchBar({
  value,
  onChange,
  placeholder = 'Search templates...',
  isSearching = false
}: SearchBarProps) {
  return (
    <div className="relative w-full" role="search">
      <label htmlFor="template-search" className="sr-only">
        Search templates
      </label>

      <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />

      <Input
        id="template-search"
        type="search"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        aria-label="Search templates by name or content"
        aria-busy={isSearching}
        className="pl-9 pr-9"
      />

      {isSearching && (
        <div className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">
          Searching...
        </div>
      )}

      {!isSearching && value && (
        <Button
          variant="ghost"
          size="sm"
          onClick={() => onChange('')}
          className="absolute right-1 top-1/2 -translate-y-1/2 h-7 w-7 p-0"
          aria-label="Clear search input"
          title="Clear search"
        >
          <X className="h-4 w-4" />
        </Button>
      )}
    </div>
  );
}
