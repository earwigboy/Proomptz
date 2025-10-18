import { useState } from 'react';
import type { FolderTreeNode } from '../../lib/api-client';
import './FolderTree.css';

interface FolderTreeProps {
  folders: FolderTreeNode[];
  selectedFolderId: string | null;
  onFolderSelect: (folderId: string | null) => void;
  onFolderContextMenu: (folderId: string, event: React.MouseEvent) => void;
  onCreateSubfolder: (parentId: string) => void;
  onTemplateDrop?: (templateId: string, targetFolderId: string | null) => void;
}

function FolderTreeItem({
  folder,
  selectedFolderId,
  onFolderSelect,
  onFolderContextMenu,
  onCreateSubfolder,
  onTemplateDrop,
  level = 0,
}: {
  folder: FolderTreeNode;
  selectedFolderId: string | null;
  onFolderSelect: (folderId: string | null) => void;
  onFolderContextMenu: (folderId: string, event: React.MouseEvent) => void;
  onCreateSubfolder: (parentId: string) => void;
  onTemplateDrop?: (templateId: string, targetFolderId: string | null) => void;
  level?: number;
}) {
  const [isExpanded, setIsExpanded] = useState(true);
  const [isDragOver, setIsDragOver] = useState(false);
  const hasChildren = folder.childFolders && folder.childFolders.length > 0;

  return (
    <div className="folder-tree-item">
      <div
        className={`folder-item ${selectedFolderId === folder.id ? 'selected' : ''} ${isDragOver ? 'drag-over' : ''}`}
        style={{ paddingLeft: `${level * 20 + 8}px` }}
        onClick={() => onFolderSelect(folder.id)}
        onContextMenu={(e) => {
          e.preventDefault();
          onFolderContextMenu(folder.id, e);
        }}
        onDragOver={(e) => {
          e.preventDefault();
          e.dataTransfer.dropEffect = 'move';
          setIsDragOver(true);
        }}
        onDragLeave={() => {
          setIsDragOver(false);
        }}
        onDrop={(e) => {
          e.preventDefault();
          setIsDragOver(false);
          const templateId = e.dataTransfer.getData('templateId');
          if (templateId && onTemplateDrop) {
            onTemplateDrop(templateId, folder.id);
          }
        }}
      >
        {hasChildren && (
          <span
            className="folder-toggle"
            onClick={(e) => {
              e.stopPropagation();
              setIsExpanded(!isExpanded);
            }}
          >
            {isExpanded ? '▼' : '▶'}
          </span>
        )}
        {!hasChildren && <span className="folder-toggle-placeholder"></span>}
        <span className="folder-icon">📁</span>
        <span className="folder-name">{folder.name}</span>
        <span className="folder-count">({folder.templateCount})</span>
      </div>
      {isExpanded && hasChildren && (
        <div className="folder-children">
          {folder.childFolders.map((child) => (
            <FolderTreeItem
              key={child.id}
              folder={child}
              selectedFolderId={selectedFolderId}
              onFolderSelect={onFolderSelect}
              onFolderContextMenu={onFolderContextMenu}
              onCreateSubfolder={onCreateSubfolder}
              onTemplateDrop={onTemplateDrop}
              level={level + 1}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export default function FolderTree({
  folders,
  selectedFolderId,
  onFolderSelect,
  onFolderContextMenu,
  onCreateSubfolder,
  onTemplateDrop,
}: FolderTreeProps) {
  const [isDragOverRoot, setIsDragOverRoot] = useState(false);

  return (
    <div className="folder-tree">
      <div className="folder-tree-header">
        <h3>Folders</h3>
        <button
          onClick={() => onCreateSubfolder('')}
          className="btn-new-folder"
          title="Create root folder"
        >
          +
        </button>
      </div>
      <div
        className={`folder-item ${selectedFolderId === null ? 'selected' : ''} ${isDragOverRoot ? 'drag-over' : ''}`}
        onClick={() => onFolderSelect(null)}
        onDragOver={(e) => {
          e.preventDefault();
          e.dataTransfer.dropEffect = 'move';
          setIsDragOverRoot(true);
        }}
        onDragLeave={() => {
          setIsDragOverRoot(false);
        }}
        onDrop={(e) => {
          e.preventDefault();
          setIsDragOverRoot(false);
          const templateId = e.dataTransfer.getData('templateId');
          if (templateId && onTemplateDrop) {
            onTemplateDrop(templateId, null);
          }
        }}
      >
        <span className="folder-icon">📂</span>
        <span className="folder-name">All Templates</span>
      </div>
      {folders.map((folder) => (
        <FolderTreeItem
          key={folder.id}
          folder={folder}
          selectedFolderId={selectedFolderId}
          onFolderSelect={onFolderSelect}
          onFolderContextMenu={onFolderContextMenu}
          onCreateSubfolder={onCreateSubfolder}
          onTemplateDrop={onTemplateDrop}
        />
      ))}
    </div>
  );
}
