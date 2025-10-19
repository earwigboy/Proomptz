import { useState, useEffect } from 'react';
import type { Folder } from '../../lib/api-client';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';

interface FolderDialogProps {
  folder: Folder | null;
  parentFolderId: string | null;
  onSave: (name: string, parentFolderId: string | null) => void;
  onClose: () => void;
  isLoading?: boolean;
}

export default function FolderDialog({
  folder,
  parentFolderId,
  onSave,
  onClose,
  isLoading = false,
}: FolderDialogProps) {
  const [name, setName] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [open, setOpen] = useState(true);

  useEffect(() => {
    if (folder) {
      setName(folder.name ?? '');
    }
  }, [folder]);

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen && !isLoading) {
      setOpen(false);
      onClose();
    }
  };

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
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>{folder ? 'Rename Folder' : 'Create New Folder'}</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit}>
          <div className="grid gap-4 py-4">
            {error && (
              <div className="text-sm text-destructive">
                {error}
              </div>
            )}

            <div className="grid gap-2">
              <Label htmlFor="folderName">Folder Name</Label>
              <Input
                id="folderName"
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter folder name"
                autoFocus
                disabled={isLoading}
                required
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
              {isLoading ? 'Saving...' : folder ? 'Rename' : 'Create'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
