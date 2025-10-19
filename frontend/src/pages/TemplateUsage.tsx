import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { TemplatesService } from '../lib/api/services/TemplatesService';
import { usePlaceholders } from '../lib/hooks/usePlaceholders';
import PlaceholderForm from '../components/placeholders/PlaceholderForm';
import PromptPreview from '../components/placeholders/PromptPreview';

export default function TemplateUsage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const { data: template, isLoading } = useQuery({
    queryKey: ['template', id],
    queryFn: () => TemplatesService.getApiTemplates1({ id: id! }),
    enabled: !!id,
  });

  const { placeholders, values, updateValue, allFilled } = usePlaceholders(
    template?.content || ''
  );

  const sendMutation = useMutation({
    mutationFn: () =>
      TemplatesService.postApiTemplatesSend({
        id: id!,
        requestBody: { placeholderValues: values },
      }),
    onSuccess: (response) => {
      setSuccess(response.message || 'Prompt sent successfully!');
      setError(null);
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'Failed to send prompt');
      setSuccess(null);
    },
  });

  if (isLoading) {
    return <div className="loading">Loading template...</div>;
  }

  if (!template) {
    return <div className="error">Template not found</div>;
  }

  return (
    <div style={{ maxWidth: '1200px', margin: '0 auto', padding: '2rem' }}>
      <div style={{ marginBottom: '2rem' }}>
        <button onClick={() => navigate('/')} style={{ marginBottom: '1rem' }}>
          ← Back to Templates
        </button>
        <h2>Use Template: {template.name}</h2>
      </div>

      {error && <div className="error-message">{error}</div>}
      {success && (
        <div
          style={{
            padding: '1rem',
            background: '#10b98133',
            border: '1px solid #10b981',
            borderRadius: '4px',
            color: '#10b981',
            marginBottom: '1rem',
          }}
        >
          {success}
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem' }}>
        <div>
          <PlaceholderForm
            placeholders={placeholders}
            values={values}
            onValueChange={updateValue}
          />
        </div>

        <div>
          <PromptPreview templateContent={template.content || ''} placeholderValues={values} />

          <div style={{ marginTop: '2rem' }}>
            <button
              onClick={() => sendMutation.mutate()}
              disabled={!allFilled || sendMutation.isPending}
              className="btn-submit"
              style={{ width: '100%' }}
            >
              {sendMutation.isPending ? 'Sending...' : 'Send to Devin'}
            </button>
            {!allFilled && placeholders.length > 0 && (
              <p style={{ color: '#888', fontSize: '0.875rem', marginTop: '0.5rem' }}>
                Please fill all placeholders to send
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
