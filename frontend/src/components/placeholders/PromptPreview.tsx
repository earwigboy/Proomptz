import { generatePrompt } from '../../lib/utils/placeholders';

interface PromptPreviewProps {
  templateContent: string;
  placeholderValues: Record<string, string>;
}

export default function PromptPreview({
  templateContent,
  placeholderValues,
}: PromptPreviewProps) {
  const generatedPrompt = generatePrompt(templateContent, placeholderValues);

  return (
    <div className="prompt-preview">
      <h3 id="prompt-preview-heading">Preview</h3>
      <div
        style={{
          background: '#1a1a1a',
          border: '1px solid #444',
          borderRadius: '8px',
          padding: '1rem',
          marginTop: '1rem',
          whiteSpace: 'pre-wrap',
          fontFamily: 'monospace',
          minHeight: '200px',
          maxHeight: '400px',
          overflow: 'auto',
        }}
        role="region"
        aria-labelledby="prompt-preview-heading"
        aria-live="polite"
        aria-atomic="true"
        tabIndex={0}
      >
        {generatedPrompt}
      </div>
    </div>
  );
}
