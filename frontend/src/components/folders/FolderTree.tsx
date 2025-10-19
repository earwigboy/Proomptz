import { useState } from 'react';
import type { FolderTreeNode } from '../../lib/api-client';
import { Button } from '@/components/ui/button';
import { ChevronDown, ChevronRight, Folder, FolderOpen, Plus } from 'lucide-react';

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
  const folderId = folder.id ?? '';
  const folderName = folder.name ?? 'Unnamed';
  const templateCount = folder.templateCount ?? 0;
  const isSelected = selectedFolderId === folderId;

  return (
    <div role="treeitem" aria-expanded={hasChildren ? isExpanded : undefined}>
      <div
        className={`
          flex items-center gap-1 py-1 px-2 rounded cursor-pointer
          hover:bg-accent transition-colors
          ${isSelected ? 'bg-accent text-accent-foreground' : ''}
          ${isDragOver ? 'bg-primary/20' : ''}
        `}
        style={{ paddingLeft: `${level * 20 + 8}px` }}
        onClick={() => onFolderSelect(folderId)}
        onContextMenu={(e) => {
          e.preventDefault();
          onFolderContextMenu(folderId, e);
        }}
        onKeyDown={(e) => {
          if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            onFolderSelect(folderId);
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
        aria-label={`Folder: ${folderName}, ${templateCount} templates`}
        aria-selected={isSelected}
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
            onTemplateDrop(templateId, folderId);
          }
        }}
      >
        {hasChildren ? (
          <Button
            variant="ghost"
            size="sm"
            className="h-5 w-5 p-0 hover:bg-transparent"
            onClick={(e) => {
              e.stopPropagation();
              setIsExpanded(!isExpanded);
            }}
            aria-label={isExpanded ? 'Collapse folder' : 'Expand folder'}
            onKeyDown={(e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                e.stopPropagation();
                setIsExpanded(!isExpanded);
              }
            }}
          >
            {isExpanded ? (
              <ChevronDown className="h-4 w-4" />
            ) : (
              <ChevronRight className="h-4 w-4" />
            )}
          </Button>
        ) : (
          <span className="w-5" />
        )}

        {isExpanded ? (
          <FolderOpen className="h-4 w-4 shrink-0 text-muted-foreground" aria-hidden="true" />
        ) : (
          <Folder className="h-4 w-4 shrink-0 text-muted-foreground" aria-hidden="true" />
        )}

        <span className="flex-1 truncate text-sm">{folderName}</span>

        <span className="text-xs text-muted-foreground" aria-label={`${templateCount} templates`}>
          ({templateCount})
        </span>
      </div>

      {isExpanded && hasChildren && (
        <div>
          {folder.childFolders?.map((child) => (
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
  const isRootSelected = selectedFolderId === null;

  return (
    <div className="p-4">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-sm font-semibold">Folders</h3>
        <Button
          variant="ghost"
          size="sm"
          className="h-7 w-7 p-0"
          onClick={() => onCreateSubfolder('')}
          title="Create root folder"
          aria-label="Create new root folder"
        >
          <Plus className="h-4 w-4" />
        </Button>
      </div>

      <nav role="tree" aria-label="Folder navigation">
        <div
          className={`
            flex items-center gap-2 py-1 px-2 rounded cursor-pointer mb-2
            hover:bg-accent transition-colors
            ${isRootSelected ? 'bg-accent text-accent-foreground' : ''}
            ${isDragOverRoot ? 'bg-primary/20' : ''}
          `}
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
          aria-selected={isRootSelected}
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
          <FolderOpen className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
          <span className="text-sm font-medium">All Templates</span>
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
