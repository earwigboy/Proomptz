// Utility functions for parsing and working with placeholders

const PLACEHOLDER_REGEX = /\{\{([a-zA-Z_][a-zA-Z0-9_]*)\}\}/g;

export function extractPlaceholders(content: string): string[] {
  const matches = content.matchAll(PLACEHOLDER_REGEX);
  const placeholders = new Set<string>();

  for (const match of matches) {
    placeholders.add(match[1]);
  }

  return Array.from(placeholders).sort();
}

export function generatePrompt(content: string, values: Record<string, string>): string {
  let result = content;

  for (const [key, value] of Object.entries(values)) {
    const placeholder = `{{${key}}}`;
    result = result.replaceAll(placeholder, value);
  }

  return result;
}

export function validatePlaceholderValues(
  requiredPlaceholders: string[],
  values: Record<string, string>
): boolean {
  return requiredPlaceholders.every(
    (placeholder) => values[placeholder] && values[placeholder].trim() !== ''
  );
}
