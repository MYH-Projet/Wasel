import { describe, it, expect } from 'vitest';
import { cn } from './utils';

describe('cn (class name utility)', () => {
  it('merges multiple class strings', () => {
    const result = cn('text-red-500', 'bg-blue-500');
    expect(result).toContain('text-red-500');
    expect(result).toContain('bg-blue-500');
  });

  it('handles conflicting Tailwind classes (last wins)', () => {
    const result = cn('text-red-500', 'text-blue-500');
    // tailwind-merge should keep only the last conflicting class
    expect(result).toBe('text-blue-500');
  });

  it('ignores falsy values', () => {
    const result = cn('base-class', false && 'hidden', null, undefined, 'visible');
    expect(result).toContain('base-class');
    expect(result).toContain('visible');
    expect(result).not.toContain('hidden');
  });

  it('returns empty string for no input', () => {
    const result = cn();
    expect(result).toBe('');
  });
});
