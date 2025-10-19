interface PlaceholderFormProps {
  placeholders: string[];
  values: Record<string, string>;
  onValueChange: (placeholder: string, value: string) => void;
}

export default function PlaceholderForm({
  placeholders,
  values,
  onValueChange,
}: PlaceholderFormProps) {
  if (placeholders.length === 0) {
    return (
      <div className="placeholder-form">
        <p style={{ color: '#888', textAlign: 'center', padding: '2rem' }}>
          This template has no placeholders to fill.
        </p>
      </div>
    );
  }

  return (
    <div className="placeholder-form">
      <h3>Fill Placeholder Values</h3>
      {placeholders.map((placeholder) => (
        <div key={placeholder} className="form-group">
          <label htmlFor={`placeholder-${placeholder}`}>
            {placeholder.replace(/_/g, ' ')}
          </label>
          <input
            id={`placeholder-${placeholder}`}
            type="text"
            value={values[placeholder] || ''}
            onChange={(e) => onValueChange(placeholder, e.target.value)}
            placeholder={`Enter value for ${placeholder}`}
          />
        </div>
      ))}
    </div>
  );
}
