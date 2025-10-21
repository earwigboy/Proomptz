import { useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { foldersApi } from '../api-client';

/**
 * Custom hook for managing folder selection state via URL search params.
 *
 * Features:
 * - Persists folder selection in URL (survives page refresh)
 * - Validates folder existence via TanStack Query
 * - Auto-clears selection if folder is deleted
 * - Session-only persistence (clears on browser close)
 *
 * @returns Object containing selected folder ID, setter function, and validation state
 */
export function useSelectedFolder() {
  const [searchParams, setSearchParams] = useSearchParams();
  const selectedFolderId = searchParams.get('folder') || null;

  // Validate folder existence if a folder is selected
  const { data: selectedFolder, isError } = useQuery({
    queryKey: ['folder', selectedFolderId],
    queryFn: () => foldersApi.getById(selectedFolderId!),
    enabled: !!selectedFolderId,
    retry: false,
  });

  // Auto-clear if folder is deleted (404 error)
  useEffect(() => {
    if (isError && selectedFolderId) {
      setSearchParams({});
    }
  }, [isError, selectedFolderId, setSearchParams]);

  /**
   * Update the selected folder ID
   * @param folderId - Folder ID to select, or null to clear selection
   */
  const setSelectedFolder = (folderId: string | null) => {
    if (folderId) {
      setSearchParams({ folder: folderId });
    } else {
      setSearchParams({});
    }
  };

  return {
    selectedFolderId,
    setSelectedFolder,
    selectedFolder,
    isError,
  };
}
