import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { foldersApi } from '../api-client';
import type { CreateFolderRequest, UpdateFolderRequest } from '../api-client';

export function useFolders() {
  const queryClient = useQueryClient();

  // Get folder tree
  const { data: folderTree, isLoading, error } = useQuery({
    queryKey: ['folders', 'tree'],
    queryFn: () => foldersApi.getTree(),
  });

  // Create folder mutation
  const createFolder = useMutation({
    mutationFn: (request: CreateFolderRequest) => foldersApi.create(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['folders'] });
      queryClient.invalidateQueries({
        queryKey: ['templates'],
        refetchType: 'active'
      });
    },
  });

  // Update folder mutation
  const updateFolder = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateFolderRequest }) =>
      foldersApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['folders'] });
      queryClient.invalidateQueries({
        queryKey: ['templates'],
        refetchType: 'active'
      });
    },
  });

  // Delete folder mutation
  const deleteFolder = useMutation({
    mutationFn: (id: string) => foldersApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['folders'] });
      queryClient.invalidateQueries({
        queryKey: ['templates'],
        refetchType: 'active'
      });
    },
  });

  // Get folder details
  const useFolderDetails = (folderId: string | null) => {
    return useQuery({
      queryKey: ['folders', folderId],
      queryFn: () => (folderId ? foldersApi.getById(folderId) : null),
      enabled: !!folderId,
    });
  };

  return {
    folderTree,
    isLoading,
    error,
    createFolder,
    updateFolder,
    deleteFolder,
    useFolderDetails,
  };
}
