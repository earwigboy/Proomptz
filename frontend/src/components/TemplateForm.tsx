import { useState, useEffect } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { templatesApi } from '../lib/api-client';
import { useFolders } from '../lib/hooks/useFolders';
import type { Template, CreateTemplateRequest, UpdateTemplateRequest, FolderTreeNode } from '../lib/api-client';

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

  useEffect(() => {
    if (template) {
      setName(template.name);
      setContent(template.content);
      setFolderId(template.folderId || null);
    } else {
      setFolderId(selectedFolderId || null);
    }
  }, [template, selectedFolderId]);

  const createMutation = useMutation({
    mutationFn: (data: CreateTemplateRequest) => templatesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['templates'] });
      queryClient.invalidateQueries({ queryKey: ['folders'] });
      onClose();
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'Failed to create template');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTemplateRequest }) =>
      templatesApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['templates'] });
      queryClient.invalidateQueries({ queryKey: ['folders'] });
      onClose();
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'Failed to update template');
    },
  });

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

    if (template) {
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
      result.push({ id: node.id, name: node.name, level });
      if (node.children && node.children.length > 0) {
        result.push(...flattenFolders(node.children, level + 1));
      }
    }
    return result;
  };

  const flatFolders = folderTree?.rootFolders ? flattenFolders(folderTree.rootFolders) : [];

  return (
    <div className="template-form">
      <div className="form-header">
        <h2>{template ? 'Edit Template' : 'Create New Template'}</h2>
        <button onClick={onClose} className="btn-close" disabled={isLoading}>
          ✕
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="name">Template Name</label>
          <input
            id="name"
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Enter template name"
            disabled={isLoading}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="folder">Folder (Optional)</label>
          <select
            id="folder"
            value={folderId || ''}
            onChange={(e) => setFolderId(e.target.value || null)}
            disabled={isLoading}
          >
            <option value="">No Folder (Root)</option>
            {flatFolders.map((folder) => (
              <option key={folder.id} value={folder.id}>
                {'\u00A0'.repeat(folder.level * 2)}
                {folder.name}
              </option>
            ))}
          </select>
        </div>

        <div className="form-group">
          <label htmlFor="content">
            Template Content
            <span className="hint">Use {'{{'}placeholder_name{'}}'} for placeholders</span>
          </label>
          <textarea
            id="content"
            value={content}
            onChange={(e) => setContent(e.target.value)}
            placeholder="Enter template content in markdown format"
            rows={15}
            disabled={isLoading}
            required
          />
        </div>

        <div className="form-actions">
          <button type="button" onClick={onClose} className="btn-cancel" disabled={isLoading}>
            Cancel
          </button>
          <button type="submit" className="btn-submit" disabled={isLoading}>
            {isLoading ? 'Saving...' : template ? 'Update Template' : 'Create Template'}
          </button>
        </div>
      </form>
    </div>
  );
}
