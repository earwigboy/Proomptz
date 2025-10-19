import { useState } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import TemplateList from './components/TemplateList';
import TemplateForm from './components/TemplateForm';
import TemplateUsage from './pages/TemplateUsage';
import Search from './pages/Search';
import FolderTree from './components/folders/FolderTree';
import FolderDialog from './components/folders/FolderDialog';
import FolderContextMenu from './components/folders/FolderContextMenu';
import { useFolders } from './lib/hooks/useFolders';
import { templatesApi } from './lib/api-client';
import type { Template, Folder } from './lib/api-client';
import './App.css';

function HomePage() {
  const queryClient = useQueryClient();
  const [editingTemplate, setEditingTemplate] = useState<Template | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [selectedFolderId, setSelectedFolderId] = useState<string | null>(null);
  const [showFolderDialog, setShowFolderDialog] = useState(false);
  const [editingFolder, setEditingFolder] = useState<Folder | null>(null);
  const [newFolderParentId, setNewFolderParentId] = useState<string | null>(null);
  const [contextMenu, setContextMenu] = useState<{
    folderId: string;
    x: number;
    y: number;
  } | null>(null);

  const { folderTree, createFolder, updateFolder, deleteFolder } = useFolders();

  const moveTemplateMutation = useMutation({
    mutationFn: async ({ templateId, folderId }: { templateId: string; folderId: string | null }) => {
      const template = await templatesApi.getById(templateId);
      return templatesApi.update(templateId, {
        name: template.name,
        content: template.content,
        folderId: folderId,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['templates'] });
      queryClient.invalidateQueries({ queryKey: ['folders'] });
    },
    onError: (error: any) => {
      alert(error.response?.data?.message || 'Failed to move template');
    },
  });

  const handleEdit = (template: Template) => {
    setEditingTemplate(template);
    setShowForm(true);
  };

  const handleCreate = () => {
    setEditingTemplate(null);
    setShowForm(true);
  };

  const handleClose = () => {
    setShowForm(false);
    setEditingTemplate(null);
  };

  const handleFolderSelect = (folderId: string | null) => {
    setSelectedFolderId(folderId);
  };

  const handleCreateFolder = (parentId: string) => {
    setNewFolderParentId(parentId || null);
    setEditingFolder(null);
    setShowFolderDialog(true);
  };

  const handleRenameFolder = (folderId: string) => {
    // Find the folder from the tree (simplified - in production you'd want a proper lookup)
    setEditingFolder({ id: folderId, name: '', parentFolderId: null, createdAt: '', updatedAt: '' } as Folder);
    setShowFolderDialog(true);
  };

  const handleDeleteFolder = (folderId: string) => {
    if (window.confirm('Are you sure you want to delete this folder? It must be empty.')) {
      deleteFolder.mutate(folderId, {
        onSuccess: () => {
          if (selectedFolderId === folderId) {
            setSelectedFolderId(null);
          }
        },
        onError: (error: any) => {
          alert(error.response?.data?.message || 'Failed to delete folder');
        },
      });
    }
  };

  const handleSaveFolder = (name: string, parentFolderId: string | null) => {
    if (editingFolder) {
      updateFolder.mutate(
        {
          id: editingFolder.id,
          data: { name, parentFolderId },
        },
        {
          onSuccess: () => {
            setShowFolderDialog(false);
            setEditingFolder(null);
            setNewFolderParentId(null);
          },
          onError: (error: any) => {
            alert(error.response?.data?.message || 'Failed to update folder');
          },
        }
      );
    } else {
      createFolder.mutate(
        { name, parentFolderId },
        {
          onSuccess: () => {
            setShowFolderDialog(false);
            setEditingFolder(null);
            setNewFolderParentId(null);
          },
          onError: (error: any) => {
            alert(error.response?.data?.message || 'Failed to create folder');
          },
        }
      );
    }
  };

  const handleFolderContextMenu = (folderId: string, event: React.MouseEvent) => {
    setContextMenu({ folderId, x: event.clientX, y: event.clientY });
  };

  const handleTemplateDrop = (templateId: string, targetFolderId: string | null) => {
    moveTemplateMutation.mutate({ templateId, folderId: targetFolderId });
  };

  const isOperationPending = moveTemplateMutation.isPending || deleteFolder.isPending;

  return (
    <div className="app-container">
      <header role="banner">
        <h1>Prompt Template Manager</h1>
      </header>
      <div className="main-content">
        <aside className="sidebar" role="complementary" aria-label="Folder navigation">
          {folderTree && (
            <FolderTree
              folders={folderTree.rootFolders}
              selectedFolderId={selectedFolderId}
              onFolderSelect={handleFolderSelect}
              onFolderContextMenu={handleFolderContextMenu}
              onCreateSubfolder={handleCreateFolder}
              onTemplateDrop={handleTemplateDrop}
            />
          )}
        </aside>
        <main className="content-area" style={{ position: 'relative' }} role="main">
          {isOperationPending && (
            <div
              style={{
                position: 'absolute',
                top: 0,
                left: 0,
                right: 0,
                background: 'rgba(100, 108, 255, 0.1)',
                border: '1px solid rgba(100, 108, 255, 0.3)',
                padding: '0.75rem',
                textAlign: 'center',
                zIndex: 100,
                color: '#646cff',
                fontSize: '0.875rem',
              }}
              role="status"
              aria-live="polite"
              aria-atomic="true"
            >
              {moveTemplateMutation.isPending && 'Moving template...'}
              {deleteFolder.isPending && 'Deleting folder...'}
            </div>
          )}
          {showForm ? (
            <TemplateForm
              template={editingTemplate}
              selectedFolderId={selectedFolderId}
              onClose={handleClose}
            />
          ) : (
            <TemplateList
              onEdit={handleEdit}
              onCreate={handleCreate}
              selectedFolderId={selectedFolderId}
            />
          )}
        </main>
      </div>

      {showFolderDialog && (
        <FolderDialog
          folder={editingFolder}
          parentFolderId={newFolderParentId}
          onSave={handleSaveFolder}
          onClose={() => {
            setShowFolderDialog(false);
            setEditingFolder(null);
            setNewFolderParentId(null);
          }}
          isLoading={createFolder.isPending || updateFolder.isPending}
        />
      )}

      {contextMenu && (
        <FolderContextMenu
          folderId={contextMenu.folderId}
          position={{ x: contextMenu.x, y: contextMenu.y }}
          onRename={() => handleRenameFolder(contextMenu.folderId)}
          onCreateSubfolder={() => handleCreateFolder(contextMenu.folderId)}
          onDelete={() => handleDeleteFolder(contextMenu.folderId)}
          onClose={() => setContextMenu(null)}
        />
      )}
    </div>
  );
}

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/search" element={<Search />} />
        <Route path="/use/:id" element={<TemplateUsage />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
