type LogLevel = 'debug' | 'info' | 'warn' | 'error';

class Logger {
  private isDevelopment = import.meta.env.DEV;
  private minLevel: LogLevel = this.isDevelopment ? 'debug' : 'warn';

  private shouldLog(level: LogLevel): boolean {
    const levels: LogLevel[] = ['debug', 'info', 'warn', 'error'];
    return levels.indexOf(level) >= levels.indexOf(this.minLevel);
  }

  debug(message: string, ...args: unknown[]): void {
    if (this.shouldLog('debug')) {
      console.debug(`[DEBUG] ${message}`, ...args);
    }
  }

  info(message: string, ...args: unknown[]): void {
    if (this.shouldLog('info')) {
      console.info(`[INFO] ${message}`, ...args);
    }
  }

  warn(message: string, ...args: unknown[]): void {
    if (this.shouldLog('warn')) {
      console.warn(`[WARN] ${message}`, ...args);
    }
  }

  error(message: string, error?: unknown, ...args: unknown[]): void {
    if (this.shouldLog('error')) {
      console.error(`[ERROR] ${message}`, error, ...args);
    }
  }

  // Log API calls
  api(method: string, url: string, status?: number, duration?: number): void {
    if (this.shouldLog('debug')) {
      const msg = `API ${method} ${url}${status ? ` - ${status}` : ''}${duration ? ` (${duration}ms)` : ''}`;
      console.log(`[API] ${msg}`);
    }
  }
}

export const logger = new Logger();
