import { useState, useEffect } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { templatesApi } from '../lib/api-client';
import { useFolders } from '../lib/hooks/useFolders';
import type { Template, CreateTemplateRequest, UpdateTemplateRequest, FolderTreeNode } from '../lib/api-client';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

interface TemplateFormProps {
  template: Template | null;
  selectedFolderId?: string | null;
  onClose: () => void;
}

export default function TemplateForm({ template, selectedFolderId, onClose }: TemplateFormProps) {
  const queryClient = useQueryClient();
  const { folderTree } = useFolders();
  const [name, setName] = useState('');
  const [content, setContent] = useState('');
  const [folderId, setFolderId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [open, setOpen] = useState(true);

  useEffect(() => {
    if (template) {
      setName(template.name ?? '');
      setContent(template.content ?? '');
      setFolderId(template.folderId ?? null);
    } else {
      setFolderId(selectedFolderId ?? null);
    }
  }, [template, selectedFolderId]);

  const createMutation = useMutation({
    mutationFn: (data: CreateTemplateRequest) => templatesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['templates'],
        refetchType: 'active'
      });
      queryClient.invalidateQueries({ queryKey: ['folders'] });
      toast.success('Template created successfully');
      onClose();
    },
    onError: (err: any) => {
      const errorMessage = err.response?.data?.message || 'Failed to create template';
      setError(errorMessage);
      toast.error(errorMessage);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTemplateRequest }) =>
      templatesApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['templates'],
        refetchType: 'active'
      });
      queryClient.invalidateQueries({ queryKey: ['folders'] });
      toast.success('Template updated successfully');
      onClose();
    },
    onError: (err: any) => {
      const errorMessage = err.response?.data?.message || 'Failed to update template';
      setError(errorMessage);
      toast.error(errorMessage);
    },
  });

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen && !isLoading) {
      setOpen(false);
      onClose();
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!name.trim()) {
      setError('Name is required');
      return;
    }

    if (!content.trim()) {
      setError('Content is required');
      return;
    }

    const data = {
      name: name.trim(),
      content: content.trim(),
      folderId: folderId,
    };

    if (template && template.id) {
      updateMutation.mutate({ id: template.id, data });
    } else {
      createMutation.mutate(data);
    }
  };

  const isLoading = createMutation.isPending || updateMutation.isPending;

  // Flatten folder tree for dropdown
  const flattenFolders = (nodes: FolderTreeNode[], level = 0): Array<{ id: string; name: string; level: number }> => {
    const result: Array<{ id: string; name: string; level: number }> = [];
    for (const node of nodes) {
      const nodeId = node.id ?? '';
      const nodeName = node.name ?? 'Unnamed';
      result.push({ id: nodeId, name: nodeName, level });
      if (node.childFolders && node.childFolders.length > 0) {
        result.push(...flattenFolders(node.childFolders, level + 1));
      }
    }
    return result;
  };

  const flatFolders = folderTree?.rootFolders ? flattenFolders(folderTree.rootFolders) : [];

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{template ? 'Edit Template' : 'Create New Template'}</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit}>
          <div className="grid gap-4 py-4">
            {error && (
              <div className="rounded-lg bg-destructive/10 p-3 text-sm text-destructive">
                {error}
              </div>
            )}

            <div className="grid gap-2">
              <Label htmlFor="name">Template Name</Label>
              <Input
                id="name"
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter template name"
                disabled={isLoading}
                required
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="folder">Folder (Optional)</Label>
              <Select
                value={folderId ?? 'root'}
                onValueChange={(value) => setFolderId(value === 'root' ? null : value)}
                disabled={isLoading}
              >
                <SelectTrigger id="folder">
                  <SelectValue placeholder="No Folder (Root)" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="root">No Folder (Root)</SelectItem>
                  {flatFolders.map((folder) => (
                    <SelectItem key={folder.id} value={folder.id}>
                      <span style={{ paddingLeft: `${folder.level * 0.75}rem` }}>
                        {folder.name}
                      </span>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="content">
                Template Content
                <span className="ml-2 text-xs text-muted-foreground">
                  Use {'{{'}placeholder_name{'}}'} for placeholders
                </span>
              </Label>
              <Textarea
                id="content"
                value={content}
                onChange={(e) => setContent(e.target.value)}
                placeholder="Enter template content in markdown format"
                rows={15}
                disabled={isLoading}
                required
                className="font-mono"
              />
            </div>
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={onClose}
              disabled={isLoading}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? 'Saving...' : template ? 'Update Template' : 'Create Template'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
