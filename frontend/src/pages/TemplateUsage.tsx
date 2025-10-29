import { useState } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { TemplatesService } from '../lib/api/services/TemplatesService';
import { usePlaceholders } from '../lib/hooks/usePlaceholders';
import { useFormValidation } from '../lib/hooks/useFormValidation';
import { useTemplateEditor } from '../lib/hooks/useTemplateEditor';
import PlaceholderForm from '../components/placeholders/PlaceholderForm';
import PromptPreview from '../components/placeholders/PromptPreview';
import TemplateEditor from '../components/template/TemplateEditor';
import { Button } from '../components/ui/button';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '../components/ui/tooltip';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../components/ui/tabs';
import { Badge } from '../components/ui/badge';

export default function TemplateUsage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'preview' | 'edit'>('preview');

  const { data: template, isLoading } = useQuery({
    queryKey: ['template', id],
    queryFn: () => TemplatesService.getApiTemplates1({ id: id! }),
    enabled: !!id,
  });

  const {
    placeholders,
    placeholderValues,
    updatePlaceholder,
    touchPlaceholder,
  } = usePlaceholders(template?.content || '');

  // T016: Integrate useFormValidation hook to compute canSubmit state
  const { canSubmit, validationMessage, missingPlaceholders } = useFormValidation(placeholders);

  // T032: Integrate useTemplateEditor hook
  const {
    finalContent,
    hasEdits,
    setEditedContent,
    resetEdits,
  } = useTemplateEditor(template?.content || '', placeholderValues);

  const [sessionUrl, setSessionUrl] = useState<string | null>(null);

  const sendMutation = useMutation({
    mutationFn: () =>
      TemplatesService.postApiTemplatesSend({
        id: id!,
        requestBody: { placeholderValues },
      }),
    onSuccess: (response) => {
      if (response.success && response.sessionUrl) {
        setSuccess(response.message || 'Prompt sent successfully!');
        setSessionUrl(response.sessionUrl);
        setError(null);
      } else {
        setError(response.message || 'Failed to send prompt');
        setSuccess(null);
        setSessionUrl(null);
      }
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'Failed to send prompt');
      setSuccess(null);
      setSessionUrl(null);
    },
  });

  if (isLoading) {
    return <div className="loading">Loading template...</div>;
  }

  if (!template) {
    return <div className="error">Template not found</div>;
  }

  return (
    <div className="h-full w-full flex flex-col" style={{ padding: '2rem' }}>
      <div className="flex-shrink-0" style={{ marginBottom: '2rem' }}>
        <button
          onClick={() => {
            const params = searchParams.toString();
            navigate(`/${params ? `?${params}` : ''}`);
          }}
          style={{ marginBottom: '1rem' }}
          aria-label="Go back to templates list"
        >
          ← Back to Templates
        </button>
        <h1>Use Template: {template.name}</h1>
      </div>

      {error && (
        <div className="error-message flex-shrink-0" role="alert" aria-live="assertive">
          {error}
        </div>
      )}
      {success && (
        <div
          className="flex-shrink-0"
          style={{
            padding: '1rem',
            background: '#10b98133',
            border: '1px solid #10b981',
            borderRadius: '4px',
            color: '#10b981',
            marginBottom: '1rem',
          }}
          role="status"
          aria-live="polite"
        >
          <div>{success}</div>
          {sessionUrl && (
            <div style={{ marginTop: '0.5rem' }}>
              <a
                href={sessionUrl}
                target="_blank"
                rel="noopener noreferrer"
                style={{
                  color: '#10b981',
                  textDecoration: 'underline',
                  fontWeight: 600,
                }}
              >
                Open Devin Session →
              </a>
            </div>
          )}
        </div>
      )}

      <div style={{
        display: 'grid',
        gridTemplateColumns: '220px 1fr',
        gap: '1rem',
        minHeight: 0,
        width: '100%',
        overflow: 'hidden'
      }}>
        {/* Placeholder form section with CSS containment to prevent layout shifts */}
        <div style={{
          minWidth: 0,
          maxWidth: '100%',
          width: '100%',
          overflow: 'auto',
          contain: 'layout size'
        }}>
          <PlaceholderForm
            placeholders={placeholders}
            onPlaceholderChange={updatePlaceholder}
            onPlaceholderBlur={touchPlaceholder}
            disabled={sendMutation.isPending}
          />
        </div>

        {/* T041 & T043: Tabs section with flex-1 (takes remaining space) and flex flex-col */}
        <div className="flex flex-col w-full" style={{
          minWidth: 0,
          minHeight: 0,
          overflow: 'hidden'
        }}>
          {/* T027-T031: Tabs component with Preview/Edit */}
          <Tabs
            value={activeTab}
            onValueChange={(value) => setActiveTab(value as 'preview' | 'edit')}
            className="flex-1 flex flex-col w-full"
          >
            <TabsList className="mb-4 flex-shrink-0">
              <TabsTrigger value="preview">Preview</TabsTrigger>
              <TabsTrigger value="edit">
                Edit
                {/* T031: Edited badge when hasEdits is true */}
                {hasEdits && (
                  <Badge variant="secondary" className="ml-2">
                    Edited
                  </Badge>
                )}
              </TabsTrigger>
            </TabsList>

            {/* T029: Preview tab content */}
            <TabsContent value="preview" className="flex-1 flex flex-col">
              {/* T045: Pass className="flex-1" to PromptPreview */}
              {/* Preview shows the finalContent (resolved OR edited) */}
              <PromptPreview
                templateContent={template.content || ''}
                placeholderValues={placeholderValues}
                className="flex-1"
                hasEdits={hasEdits}
              />
            </TabsContent>

            {/* T030: Edit tab content */}
            <TabsContent value="edit" className="flex-1 flex flex-col">
              {/* T033: Pass finalContent to TemplateEditor */}
              {/* T034: Wire setEditedContent callback */}
              {/* T035: Wire resetEdits callback */}
              <TemplateEditor
                content={finalContent}
                onContentChange={setEditedContent}
                hasEdits={hasEdits}
                onReset={resetEdits}
                disabled={sendMutation.isPending}
              />
            </TabsContent>
          </Tabs>

          {/* T044: Send button section with flex-shrink-0 (fixed height) */}
          <div className="flex-shrink-0" style={{ marginTop: '2rem' }}>
            {/* T018-T022: Tooltip wrapper with validation feedback and accessibility */}
            <TooltipProvider>
              <Tooltip>
                <TooltipTrigger asChild>
                  {/* T017: Button with canSubmit state, T022: shadcn/ui Button with disabled styling */}
                  <Button
                    onClick={() => sendMutation.mutate()}
                    disabled={!canSubmit || sendMutation.isPending}
                    className="w-full"
                    size="lg"
                    aria-busy={sendMutation.isPending}
                    aria-describedby={!canSubmit ? 'send-button-status' : undefined}
                  >
                    {sendMutation.isPending ? 'Sending...' : 'Send to Devin'}
                  </Button>
                </TooltipTrigger>
                {/* T019: Tooltip content shows validationMessage when disabled */}
                <TooltipContent>
                  <p>{validationMessage ?? 'Send template to Devin'}</p>
                </TooltipContent>
              </Tooltip>
            </TooltipProvider>

            {/* T021: Screen reader status message with aria-live */}
            {!canSubmit && (
              <p
                id="send-button-status"
                className="text-sm text-muted-foreground mt-2 sr-only"
                aria-live="polite"
              >
                Button disabled: {missingPlaceholders.length} placeholder(s) required
              </p>
            )}

            {/* Visual feedback for sighted users */}
            {!canSubmit && placeholders.length > 0 && (
              <p
                className="text-sm text-muted-foreground mt-2 text-center"
                role="status"
              >
                Please fill all required placeholders to send
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
