# Service Worker & Progressive Web App (PWA) Setup Guide

## Overview
This guide helps implement offline-first caching strategy with Angular Service Worker for the BusinessNewEnvironment application.

## Quick Setup (Angular)

### 1. Add Service Worker Support
```bash
ng add @angular/service-worker
```

### 2. Configure in `angular.json`
Add to `projects > [project-name] > architect > build`:
```json
{
  "serviceWorker": true,
  "ngswConfigPath": "ngsw-config.json"
}
```

### 3. Enable Service Worker in `main.ts`
```typescript
import { bootstrapApplication } from '@angular/platform-browser';
import { provideServiceWorker } from '@angular/service-worker';
import { AppComponent } from './app/app.component';
import { environment } from './environments/environment';

bootstrapApplication(AppComponent, {
  providers: [
	provideServiceWorker('ngsw-worker.js', {
	  enabled: environment.production,
	  registrationStrategy: 'registerWhenStable:30000'
	})
  ]
});
```

### 4. Environment Configuration
In `environment.prod.ts`:
```typescript
export const environment = {
  production: true,
  serviceWorker: true
};
```

## Caching Strategies

### Strategy 1: Freshness First (API Data)
- Tries network first
- Falls back to cache if network unavailable
- **Best for**: Categories, search, business details (not frequently changing)
- **TTL**: 1hr for categories, 5min for search

### Strategy 2: Cache First (Immutable Assets)
- Uses cache first
- Updates cache in background
- **Best for**: Images, fonts, versioned JS/CSS bundles
- **TTL**: 30 days

### Strategy 3: Network Only (Auth/Mutations)
- Always fetch from network
- No caching
- **Best for**: Login, registration, POST/PUT/DELETE endpoints
- **TTL**: N/A

## Cache Management

### Automatic Cache Cleanup
Service Worker automatically manages cache size. Configure in `ngsw-config.json`:
- `maxSize`: Max number of entries per cache group
- `maxAge`: Time before cache entry expires
- Oldest entries removed first (LRU strategy)

### Manual Cache Invalidation
```typescript
import { SwUpdate } from '@angular/service-worker';

constructor(swUpdate: SwUpdate) {
  swUpdate.versionUpdates.subscribe(evt => {
	if (evt.type === 'VERSION_READY') {
	  if (confirm('New version available. Reload?')) {
		document.location.reload();
	  }
	}
  });
}
```

## Offline Support

### Offline Indicator Component
```typescript
import { SwUpdate } from '@angular/service-worker';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class OfflineService {
  offline$ = new BehaviorSubject(false);

  constructor() {
	window.addEventListener('offline', () => this.offline$.next(true));
	window.addEventListener('online', () => this.offline$.next(false));
  }
}
```

### Offline Page
Create `offline.html` in `src/assets/`:
```html
<!DOCTYPE html>
<html>
<head>
	<title>Offline</title>
	<style>
		body { font-family: Arial; text-align: center; padding: 50px; }
		h1 { color: #333; }
	</style>
</head>
<body>
	<h1>You are offline</h1>
	<p>Check your internet connection and refresh the page.</p>
</body>
</html>
```

## Performance Metrics

Expected improvements with PWA implementation:
- **First Load**: 40-50% faster (cached app shell)
- **Subsequent Loads**: 80-90% faster (cache hits)
- **Bandwidth Reduction**: 60-70% (compression + caching)
- **Offline Availability**: 100% (app shell + cached data)
- **Network Resilience**: Auto-falls back to cache on connection issues

## Monitoring

### Angular DevTools
```typescript
// Check service worker status
navigator.serviceWorker.getRegistrations().then(registrations => {
  console.log('Service Workers:', registrations);
});
```

### Chrome DevTools
- Application > Service Workers: View registration and cache storage
- Check "Update on reload" for development
- Unregister and hard refresh to test fresh install

## Database (Server-Side)

### Query Caching
- Categories: Cached for 1 hour (rarely change)
- Subcategories: Cached for 1 hour
- Search results: Cached for 5 minutes

### Implementing HTTP Cache-Control
Already implemented via ResponseCache attributes in controllers.

## Best Practices

1. **Version Cache Names**: Include version (app-shell-v1)
2. **Test Offline**: Disable network in DevTools, verify fallback
3. **Monitor Cache Size**: Set maxSize limits in ngsw-config.json
4. **Update Strategy**: Use registerWhenStable with timeout
5. **Clear Old Caches**: Implement cleanup in service worker

## Troubleshooting

### Service Worker Not Registering
```typescript
// Check browser console for errors
navigator.serviceWorker.getRegistrations().then(r => 
  r.forEach(reg => console.log(reg))
);
```

### Cache Not Updating
- Build with `ng build --prod`
- Service worker only works in production builds
- Updated every time you deploy (version hash in ngsw.json)

### Clear Cache Manually
```javascript
// In DevTools Console
caches.keys().then(names => {
  names.forEach(name => caches.delete(name));
});
```

## References
- [@angular/service-worker docs](https://angular.io/guide/service-worker-intro)
- [Service Worker API](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API)
- [PWA Checklist](https://web.dev/pwa-checklist/)
