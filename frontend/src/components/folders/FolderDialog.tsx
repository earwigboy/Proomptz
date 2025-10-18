import { useState, useEffect } from 'react';
import type { Folder } from '../../lib/api-client';
import './FolderDialog.css';

interface FolderDialogProps {
  folder: Folder | null;
  parentFolderId: string | null;
  onSave: (name: string, parentFolderId: string | null) => void;
  onClose: () => void;
}

export default function FolderDialog({
  folder,
  parentFolderId,
  onSave,
  onClose,
}: FolderDialogProps) {
  const [name, setName] = useState('');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (folder) {
      setName(folder.name);
    }
  }, [folder]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    const trimmedName = name.trim();
    if (!trimmedName) {
      setError('Folder name is required');
      return;
    }

    if (trimmedName.length > 100) {
      setError('Folder name must not exceed 100 characters');
      return;
    }

    // Check for invalid characters
    const invalidChars = /[/\\:*?"<>|]/;
    if (invalidChars.test(trimmedName)) {
      setError('Folder name contains invalid characters');
      return;
    }

    onSave(trimmedName, folder?.parentFolderId ?? parentFolderId);
    setName('');
  };

  return (
    <div className="folder-dialog-overlay" onClick={onClose}>
      <div className="folder-dialog" onClick={(e) => e.stopPropagation()}>
        <div className="dialog-header">
          <h2>{folder ? 'Rename Folder' : 'Create New Folder'}</h2>
          <button onClick={onClose} className="btn-close">
            ✕
          </button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="folderName">Folder Name</label>
            <input
              id="folderName"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Enter folder name"
              autoFocus
              required
            />
          </div>

          <div className="dialog-actions">
            <button type="button" onClick={onClose} className="btn-cancel">
              Cancel
            </button>
            <button type="submit" className="btn-submit">
              {folder ? 'Rename' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
