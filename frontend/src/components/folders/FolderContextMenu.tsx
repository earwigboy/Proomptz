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
    >
      <button
        className="context-menu-item"
        onClick={() => {
          onRename();
          onClose();
        }}
      >
        <span className="menu-icon">✏️</span>
        Rename
      </button>
      <button
        className="context-menu-item"
        onClick={() => {
          onCreateSubfolder();
          onClose();
        }}
      >
        <span className="menu-icon">📁</span>
        New Subfolder
      </button>
      <div className="context-menu-divider"></div>
      <button
        className="context-menu-item danger"
        onClick={() => {
          onDelete();
          onClose();
        }}
      >
        <span className="menu-icon">🗑️</span>
        Delete
      </button>
    </div>
  );
}
