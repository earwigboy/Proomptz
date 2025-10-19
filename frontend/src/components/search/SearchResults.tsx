import { useNavigate } from 'react-router-dom';
import type { TemplateSummary } from '../../lib/api-client';
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Folder } from 'lucide-react';

interface SearchResultsProps {
  results: TemplateSummary[];
  isLoading: boolean;
  query: string;
}

function SearchResultSkeleton() {
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-6 w-3/4" />
        <Skeleton className="h-4 w-1/2 mt-2" />
      </CardHeader>
      <CardContent>
        <div className="space-y-2">
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-2/3" />
        </div>
      </CardContent>
      <CardFooter className="flex justify-between">
        <Skeleton className="h-4 w-32" />
        <Skeleton className="h-9 w-28" />
      </CardFooter>
    </Card>
  );
}

export default function SearchResults({ results, isLoading, query }: SearchResultsProps) {
  const navigate = useNavigate();

  if (isLoading) {
    return (
      <div className="search-results">
        <div className="text-muted-foreground mb-4" role="status" aria-live="polite">
          Searching...
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4" role="list">
          {Array.from({ length: 6 }).map((_, i) => (
            <SearchResultSkeleton key={i} />
          ))}
        </div>
      </div>
    );
  }

  if (!query) {
    return (
      <div className="text-center py-12">
        <p className="text-muted-foreground">Enter a search query to find templates</p>
      </div>
    );
  }

  if (results.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-muted-foreground">No templates found for "{query}"</p>
      </div>
    );
  }

  return (
    <div className="search-results">
      <div className="text-muted-foreground mb-4" role="status" aria-live="polite">
        <p>Found {results.length} result{results.length !== 1 ? 's' : ''} for "{query}"</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4" role="list" aria-label="Search results">
        {results.map((template) => (
          <Card
            key={template.id}
            className="cursor-pointer transition-colors hover:border-primary/50"
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
            <CardHeader>
              <CardTitle className="text-lg">{template.name}</CardTitle>
              {template.folderName && (
                <div className="flex items-center gap-1 text-sm text-muted-foreground mt-1">
                  <Folder className="h-3 w-3" />
                  <span>{template.folderName}</span>
                </div>
              )}
            </CardHeader>

            <CardContent>
              <CardDescription className="line-clamp-3">
                {template.contentPreview}
                {template.contentPreview && template.contentPreview.length >= 200 && '...'}
              </CardDescription>
            </CardContent>

            <CardFooter className="flex justify-between items-center">
              <div className="text-xs text-muted-foreground">
                Updated: {new Date(template.updatedAt || '').toLocaleDateString()}
              </div>
              <Button
                size="sm"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/use/${template.id}`);
                }}
                aria-label={`Use template ${template.name}`}
              >
                Use Template
              </Button>
            </CardFooter>
          </Card>
        ))}
      </div>
    </div>
  );
}
