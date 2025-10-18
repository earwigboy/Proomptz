import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { templatesApi } from '../lib/api-client';
import type { Template } from '../lib/api-client';

interface TemplateListProps {
  onEdit: (template: Template) => void;
  onCreate: () => void;
  selectedFolderId?: string | null;
}

export default function TemplateList({ onEdit, onCreate, selectedFolderId }: TemplateListProps) {
  const queryClient = useQueryClient();

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
    },
  });

  const handleDelete = async (id: string, name: string) => {
    if (window.confirm(`Are you sure you want to delete "${name}"?`)) {
      try {
        await deleteMutation.mutateAsync(id);
      } catch (err) {
        alert('Failed to delete template');
      }
    }
  };

  if (isLoading) {
    return <div className="loading">Loading templates...</div>;
  }

  if (error) {
    return (
      <div className="error">
        Error loading templates: {error instanceof Error ? error.message : 'Unknown error'}
      </div>
    );
  }

  const templates = data?.items || [];

  return (
    <div className="template-list">
      <div className="list-header">
        <h2>Templates</h2>
        <button onClick={onCreate} className="btn-create">
          Create New Template
        </button>
      </div>

      {templates.length === 0 ? (
        <div className="empty-state">
          <p>
            {selectedFolderId
              ? 'No templates in this folder yet.'
              : 'No templates yet. Create your first template to get started.'}
          </p>
        </div>
      ) : (
        <div className="templates-grid">
          {templates.map((template) => (
            <div
              key={template.id}
              className="template-card"
              draggable
              onDragStart={(e) => {
                e.dataTransfer.setData('templateId', template.id);
                e.dataTransfer.effectAllowed = 'move';
              }}
            >
              <div className="template-header">
                <h3>{template.name}</h3>
                <div className="template-meta">
                  {template.placeholderCount > 0 && (
                    <span className="placeholder-count">
                      {template.placeholderCount} placeholder{template.placeholderCount !== 1 ? 's' : ''}
                    </span>
                  )}
                </div>
              </div>

              <div className="template-preview">
                {template.contentPreview}
                {template.contentPreview.length >= 200 && '...'}
              </div>

              <div className="template-footer">
                <div className="template-dates">
                  <small>Updated: {new Date(template.updatedAt).toLocaleDateString()}</small>
                </div>
                <div className="template-actions">
                  <button
                    onClick={async () => {
                      const fullTemplate = await templatesApi.getById(template.id);
                      onEdit(fullTemplate);
                    }}
                    className="btn-edit"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => handleDelete(template.id, template.name)}
                    className="btn-delete"
                    disabled={deleteMutation.isPending}
                  >
                    Delete
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
