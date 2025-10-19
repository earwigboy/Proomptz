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
    <div className="folder-tree-item" role="treeitem" aria-expanded={hasChildren ? isExpanded : undefined}>
      <div
        className={`folder-item ${selectedFolderId === folder.id ? 'selected' : ''} ${isDragOver ? 'drag-over' : ''}`}
        style={{ paddingLeft: `${level * 20 + 8}px` }}
        onClick={() => onFolderSelect(folder.id)}
        onContextMenu={(e) => {
          e.preventDefault();
          onFolderContextMenu(folder.id, e);
        }}
        onKeyDown={(e) => {
          if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            onFolderSelect(folder.id);
          } else if (e.key === 'ArrowRight' && hasChildren && !isExpanded) {
            e.preventDefault();
            setIsExpanded(true);
          } else if (e.key === 'ArrowLeft' && isExpanded) {
            e.preventDefault();
            setIsExpanded(false);
          }
        }}
        tabIndex={0}
        role="button"
        aria-label={`Folder: ${folder.name}, ${folder.templateCount} templates`}
        aria-selected={selectedFolderId === folder.id}
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
            role="button"
            aria-label={isExpanded ? 'Collapse folder' : 'Expand folder'}
            tabIndex={0}
            onKeyDown={(e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                e.stopPropagation();
                setIsExpanded(!isExpanded);
              }
            }}
          >
            {isExpanded ? '▼' : '▶'}
          </span>
        )}
        {!hasChildren && <span className="folder-toggle-placeholder"></span>}
        <span className="folder-icon" aria-hidden="true">📁</span>
        <span className="folder-name">{folder.name}</span>
        <span className="folder-count" aria-label={`${folder.templateCount} templates`}>
          ({folder.templateCount})
        </span>
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
          aria-label="Create new root folder"
        >
          +
        </button>
      </div>
      <nav role="tree" aria-label="Folder navigation">
        <div
          className={`folder-item ${selectedFolderId === null ? 'selected' : ''} ${isDragOverRoot ? 'drag-over' : ''}`}
          onClick={() => onFolderSelect(null)}
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault();
              onFolderSelect(null);
            }
          }}
          tabIndex={0}
          role="treeitem"
          aria-label="All Templates root folder"
          aria-selected={selectedFolderId === null}
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
          <span className="folder-icon" aria-hidden="true">📂</span>
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
      </nav>
    </div>
  );
}
