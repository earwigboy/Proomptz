import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { templatesApi } from '../lib/api-client';
import type { Template } from '../lib/api-client';
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { AlertCircle } from 'lucide-react';

interface TemplateListProps {
  onEdit: (template: Template) => void;
  onCreate: () => void;
  selectedFolderId?: string | null;
}

function TemplateCardSkeleton() {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-start justify-between">
          <Skeleton className="h-6 w-3/4" />
          <Skeleton className="h-5 w-16" />
        </div>
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
        <div className="flex gap-2">
          <Skeleton className="h-9 w-16" />
          <Skeleton className="h-9 w-16" />
          <Skeleton className="h-9 w-20" />
        </div>
      </CardFooter>
    </Card>
  );
}

export default function TemplateList({ onEdit, onCreate, selectedFolderId }: TemplateListProps) {
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [loadingTemplateId, setLoadingTemplateId] = useState<string | null>(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [templateToDelete, setTemplateToDelete] = useState<{ id: string; name: string } | null>(null);

  const { data, isLoading, error } = useQuery({
    queryKey: ['templates', selectedFolderId],
    queryFn: () => {
      // If a folder is selected, filter by that folder
      // If null/undefined, the API will return all templates
      if (selectedFolderId) {
        return templatesApi.getAll({ folderId: selectedFolderId });
      }
      return templatesApi.getAll();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => templatesApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['templates'] });
      queryClient.invalidateQueries({ queryKey: ['folders'] });
      toast.success('Template deleted successfully');
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.message || 'Failed to delete template');
    },
  });

  const handleDelete = (id: string, name: string) => {
    setTemplateToDelete({ id, name });
    setDeleteDialogOpen(true);
  };

  const confirmDelete = async () => {
    if (!templateToDelete) return;

    try {
      await deleteMutation.mutateAsync(templateToDelete.id);
      setDeleteDialogOpen(false);
      setTemplateToDelete(null);
    } catch (err) {
      // Error is already handled by mutation's onError with toast
      setDeleteDialogOpen(false);
      setTemplateToDelete(null);
    }
  };

  if (isLoading) {
    return (
      <div className="template-list">
        <div className="list-header mb-6">
          <h2 className="text-2xl font-bold">Templates</h2>
          <div className="flex gap-4">
            <Button
              variant="outline"
              onClick={() => navigate('/search')}
              aria-label="Search templates"
            >
              🔍 Search
            </Button>
            <Button onClick={onCreate} aria-label="Create new template">
              Create New Template
            </Button>
          </div>
        </div>
        <div className="templates-grid grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4" role="list">
          {Array.from({ length: 6 }).map((_, i) => (
            <TemplateCardSkeleton key={i} />
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="template-list">
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertTitle>Error</AlertTitle>
          <AlertDescription>
            Error loading templates: {error instanceof Error ? error.message : 'Unknown error'}
          </AlertDescription>
        </Alert>
      </div>
    );
  }

  const templates = data?.items || [];

  return (
    <div className="template-list">
      <div className="list-header mb-6">
        <h2 className="text-2xl font-bold">Templates</h2>
        <div className="flex gap-4">
          <Button
            variant="outline"
            onClick={() => navigate('/search')}
            aria-label="Search templates"
          >
            🔍 Search
          </Button>
          <Button onClick={onCreate} aria-label="Create new template">
            Create New Template
          </Button>
        </div>
      </div>

      {templates.length === 0 ? (
        <div className="empty-state py-12 text-center">
          <p className="text-muted-foreground">
            {selectedFolderId
              ? 'No templates in this folder yet.'
              : 'No templates yet. Create your first template to get started.'}
          </p>
        </div>
      ) : (
        <div className="templates-grid grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4" role="list">
          {templates.map((template) => (
            <Card
              key={template.id}
              className="transition-colors hover:border-primary/50"
              draggable
              onDragStart={(e) => {
                e.dataTransfer.setData('templateId', template.id ?? '');
                e.dataTransfer.effectAllowed = 'move';
              }}
              role="listitem"
              aria-label={`Template: ${template.name}`}
            >
              <CardHeader>
                <div className="flex items-start justify-between gap-2">
                  <CardTitle className="text-lg">{template.name}</CardTitle>
                  {(template.placeholderCount ?? 0) > 0 && (
                    <Badge variant="secondary" className="shrink-0">
                      {template.placeholderCount} placeholder{template.placeholderCount !== 1 ? 's' : ''}
                    </Badge>
                  )}
                </div>
              </CardHeader>

              <CardContent>
                <CardDescription className="line-clamp-3">
                  {template.contentPreview}
                  {(template.contentPreview?.length ?? 0) >= 200 && '...'}
                </CardDescription>
              </CardContent>

              <CardFooter className="flex flex-col gap-3">
                <div className="text-sm text-muted-foreground w-full">
                  Updated: {new Date(template.updatedAt ?? '').toLocaleDateString()}
                </div>
                <div className="flex gap-2 w-full" role="group" aria-label="Template actions">
                  <Button
                    size="sm"
                    onClick={() => navigate(`/use/${template.id ?? ''}`)}
                    aria-label={`Use template ${template.name}`}
                  >
                    Use
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={async () => {
                      const id = template.id ?? '';
                      setLoadingTemplateId(id);
                      try {
                        const fullTemplate = await templatesApi.getById(id);
                        onEdit(fullTemplate);
                      } catch (err) {
                        alert('Failed to load template');
                      } finally {
                        setLoadingTemplateId(null);
                      }
                    }}
                    disabled={loadingTemplateId === template.id}
                    aria-label={`Edit template ${template.name}`}
                    aria-busy={loadingTemplateId === template.id}
                  >
                    {loadingTemplateId === template.id ? 'Loading...' : 'Edit'}
                  </Button>
                  <Button
                    size="sm"
                    variant="destructive"
                    onClick={() => handleDelete(template.id ?? '', template.name ?? 'template')}
                    disabled={deleteMutation.isPending}
                    aria-label={`Delete template ${template.name}`}
                    aria-busy={deleteMutation.isPending}
                  >
                    Delete
                  </Button>
                </div>
              </CardFooter>
            </Card>
          ))}
        </div>
      )}

      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Template</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete "{templateToDelete?.name}"? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={deleteMutation.isPending}>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={confirmDelete}
              disabled={deleteMutation.isPending}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {deleteMutation.isPending ? 'Deleting...' : 'Delete'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
