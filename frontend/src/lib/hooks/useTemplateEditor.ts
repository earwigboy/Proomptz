import { useState, useMemo, useCallback } from 'react';
import { generatePrompt } from '../utils/placeholders';
import type { UseTemplateEditorResult } from '../types/templateEditor';

/**
 * Hook for managing template content editing state
 *
 * @param templateContent - Original template content from API
 * @param placeholderValues - Current placeholder values from usePlaceholders
 * @returns Template editor state and actions
 */
export function useTemplateEditor(
  templateContent: string,
  placeholderValues: Record<string, string>
): UseTemplateEditorResult {
  const [editedContent, setEditedContentState] = useState<string | null>(null);

  // Compute resolved content (template with placeholders substituted)
  const resolvedContent = useMemo(
    () => generatePrompt(templateContent, placeholderValues),
    [templateContent, placeholderValues]
  );

  // Final content is edited content if it exists, otherwise resolved content
  const finalContent = editedContent ?? resolvedContent;

  // Has edits if edited content is not null
  const hasEdits = editedContent !== null;

  const setEditedContent = useCallback((content: string) => {
    setEditedContentState(content);
  }, []);

  const resetEdits = useCallback(() => {
    setEditedContentState(null);
  }, []);

  const getContentToSend = useCallback(() => {
    return finalContent;
  }, [finalContent]);

  return {
    resolvedContent,
    finalContent,
    hasEdits,
    setEditedContent,
    resetEdits,
    getContentToSend,
  };
}
