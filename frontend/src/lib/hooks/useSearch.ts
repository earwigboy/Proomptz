import { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { SearchService } from '../api/services/SearchService';

export function useSearch(query: string, page: number = 1, pageSize: number = 50) {
  const [debouncedQuery, setDebouncedQuery] = useState(query);

  // Debounce the search query
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedQuery(query);
    }, 300); // 300ms debounce

    return () => clearTimeout(timer);
  }, [query]);

  const { data, isLoading, error } = useQuery({
    queryKey: ['search', debouncedQuery, page, pageSize],
    queryFn: () => SearchService.getApiSearch({ q: debouncedQuery, page, pageSize }),
    enabled: debouncedQuery.length > 0,
  });

  return {
    results: data?.items || [],
    totalCount: data?.totalCount || 0,
    hasMore: data?.hasMore || false,
    isLoading,
    error,
    isSearching: query !== debouncedQuery,
  };
}
