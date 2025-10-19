import { useEffect, useRef } from 'react';
import './FolderContextMenu.css';

interface FolderContextMenuProps {
  folderId: string;
  position: { x: number; y: number };
  onRename: () => void;
  onCreateSubfolder: () => void;
  onDelete: () => void;
  onClose: () => void;
}

export default function FolderContextMenu({
  folderId,
  position,
  onRename,
  onCreateSubfolder,
  onDelete,
  onClose,
}: FolderContextMenuProps) {
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        onClose();
      }
    };

    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onClose();
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    document.addEventListener('keydown', handleEscape);

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      document.removeEventListener('keydown', handleEscape);
    };
  }, [onClose]);

  return (
    <div
      ref={menuRef}
      className="folder-context-menu"
      style={{
        top: `${position.y}px`,
        left: `${position.x}px`,
      }}
      role="menu"
      aria-label="Folder actions menu"
    >
      <button
        className="context-menu-item"
        onClick={() => {
          onRename();
          onClose();
        }}
        role="menuitem"
        aria-label="Rename folder"
      >
        <span className="menu-icon" aria-hidden="true">✏️</span>
        Rename
      </button>
      <button
        className="context-menu-item"
        onClick={() => {
          onCreateSubfolder();
          onClose();
        }}
        role="menuitem"
        aria-label="Create new subfolder"
      >
        <span className="menu-icon" aria-hidden="true">📁</span>
        New Subfolder
      </button>
      <div className="context-menu-divider" role="separator"></div>
      <button
        className="context-menu-item danger"
        onClick={() => {
          onDelete();
          onClose();
        }}
        role="menuitem"
        aria-label="Delete folder"
      >
        <span className="menu-icon" aria-hidden="true">🗑️</span>
        Delete
      </button>
    </div>
  );
}
