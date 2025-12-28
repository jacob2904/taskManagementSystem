/**
 * Convert UTC datetime string from backend to local time for datetime-local input
 * Backend stores dates in UTC, but datetime-local input expects local time
 */
export function utcToIsraelLocal(dateString: string): string {
  // Parse the UTC datetime string
  const utcDate = new Date(dateString + 'Z'); // Add 'Z' to indicate UTC
  
  // Format for datetime-local input (YYYY-MM-DDTHH:mm)
  const year = utcDate.getFullYear();
  const month = String(utcDate.getMonth() + 1).padStart(2, '0');
  const day = String(utcDate.getDate()).padStart(2, '0');
  const hours = String(utcDate.getHours()).padStart(2, '0');
  const minutes = String(utcDate.getMinutes()).padStart(2, '0');
  
  return `${year}-${month}-${day}T${hours}:${minutes}`;
}

/**
 * Format UTC datetime for display in local timezone
 */
export function formatIsraelDateTime(dateString: string): string {
  // Parse the UTC datetime string
  const utcDate = new Date(dateString + 'Z'); // Add 'Z' to indicate UTC
  
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  const monthName = months[utcDate.getMonth()];
  const day = utcDate.getDate();
  const year = utcDate.getFullYear();
  const hours = String(utcDate.getHours()).padStart(2, '0');
  const minutes = String(utcDate.getMinutes()).padStart(2, '0');
  
  return `${monthName} ${day}, ${year} • ${hours}:${minutes}`;
}

/**
 * Get current local time for datetime-local input default
 * Uses the browser's local timezone
 */
export function getCurrentIsraelTime(): string {
  const now = new Date();
  
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  const hours = String(now.getHours()).padStart(2, '0');
  const minutes = String(now.getMinutes()).padStart(2, '0');
  
  return `${year}-${month}-${day}T${hours}:${minutes}`;
}

/**
 * Convert local datetime-local input value to UTC ISO string for backend
 * Backend expects UTC datetime without 'Z' suffix
 */
export function localToUtc(localDateTimeString: string): string {
  // Parse the local datetime string (format: YYYY-MM-DDTHH:mm)
  const localDate = new Date(localDateTimeString);
  
  // Convert to UTC and format as ISO string without 'Z'
  const year = localDate.getUTCFullYear();
  const month = String(localDate.getUTCMonth() + 1).padStart(2, '0');
  const day = String(localDate.getUTCDate()).padStart(2, '0');
  const hours = String(localDate.getUTCHours()).padStart(2, '0');
  const minutes = String(localDate.getUTCMinutes()).padStart(2, '0');
  const seconds = String(localDate.getUTCSeconds()).padStart(2, '0');
  
  return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;
}
