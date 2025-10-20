/**
 * Type definitions for template content editing
 */

/**
 * State for template content editing
 */
export interface TemplateEditorState {
  /** Original template content from API */
  originalContent: string;

  /** Template with placeholders substituted */
  resolvedContent: string;

  /** User-modified content (null if not edited) */
  editedContent: string | null;

  /** Whether the user has made edits */
  hasEdits: boolean;

  /** Final content to display/send (edited OR resolved) */
  finalContent: string;
}

/**
 * Return type for useTemplateEditor hook
 */
export interface UseTemplateEditorResult {
  /** Template content with placeholders substituted */
  resolvedContent: string;

  /** Final content (edited if user modified, otherwise resolved) */
  finalContent: string;

  /** Whether user has made edits to the template */
  hasEdits: boolean;

  /** Set edited content (switches to edit mode) */
  setEditedContent: (content: string) => void;

  /** Clear edits and revert to resolved content */
  resetEdits: () => void;

  /** Get content that should be sent to Devin */
  getContentToSend: () => string;
}
