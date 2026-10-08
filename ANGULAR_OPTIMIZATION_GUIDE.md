# Angular Performance Optimization Guide

## Lazy-Loading Routes Configuration

This guide provides the recommended module structure and lazy-loading strategy for the BusinessNewEnvironment application.

### Recommended Module Structure

```
src/
  app/
	core/                          # Singleton services, guards, interceptors
	  guards/
		auth.guard.ts
	  interceptors/
		http-caching.interceptor.ts
		error.interceptor.ts
	  services/
		auth.service.ts
		business.service.ts
	shared/                        # Shared components, directives, pipes
	  components/
		navbar/
		footer/
	features/
	  auth/                        # Lazy-loaded
		auth.module.ts
		auth-routing.module.ts
		pages/
		  login.component.ts
		  register.component.ts
	  business/                    # Lazy-loaded
		business.module.ts
		business-routing.module.ts
		pages/
		  search.component.ts
		  detail.component.ts
		  list.component.ts
	  admin/                       # Lazy-loaded
		admin.module.ts
		admin-routing.module.ts
		dashboards/
		  admin-dashboard.component.ts
	  public/                      # Main/public module
		public.module.ts
		pages/
		  home.component.ts
	app.routing.module.ts
	app.module.ts
```

### Main Routing Module (app.routing.module.ts)

```typescript
import { NgModule } from '@angular/core';
import { RouterModule, Routes, PreloadAllModules } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  { path: '', loadChildren: () => import('./features/public/public.module').then(m => m.PublicModule) },

  // Lazy-loaded auth module
  { 
	path: 'auth', 
	loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  },

  // Lazy-loaded business module
  { 
	path: 'businesses', 
	loadChildren: () => import('./features/business/business.module').then(m => m.BusinessModule),
	data: { preload: true } // Preload this route after app initializes (lower priority)
  },

  // Lazy-loaded admin module (protected)
  { 
	path: 'admin', 
	canActivate: [AuthGuard],
	data: { roles: ['admin'] },
	loadChildren: () => import('./features/admin/admin.module').then(m => m.AdminModule),
	data: { preload: true }
  },

  // Wildcard route
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {
	// Preload strategy: Preload marked routes after app initializes
	preloadingStrategy: PreloadAllModules,
	// Enable route tracing in development
	enableTracing: false,
	// Use hash location strategy for GitHub Pages compatibility
	useHash: true,
	// Initial navigation timing
	initialNavigation: 'enabledBlocking',
	// Scroll position strategy
	scrollPositionRestoration: 'top',
	// Anchor scrolling
	anchorScrolling: 'enabled',
	// Relative link resolution
	relativeLinkResolution: 'corrected'
  })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
```

### Business Module Routing (features/business/business-routing.module.ts)

```typescript
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BusinessSearchComponent } from './pages/search.component';
import { BusinessDetailComponent } from './pages/detail.component';
import { BusinessListComponent } from './pages/list.component';

const routes: Routes = [
  { 
	path: '', 
	component: BusinessListComponent,
	data: { title: 'Businesses' }
  },
  { 
	path: 'search', 
	component: BusinessSearchComponent,
	data: { title: 'Search Businesses' }
  },
  { 
	path: ':id', 
	component: BusinessDetailComponent,
	data: { title: 'Business Details' }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class BusinessRoutingModule { }
```

### Admin Module Routing (features/admin/admin-routing.module.ts)

```typescript
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminDashboardComponent } from './dashboards/admin-dashboard.component';

const routes: Routes = [
  { 
	path: '', 
	component: AdminDashboardComponent,
	data: { title: 'Admin Dashboard' }
  },
  // Additional admin routes...
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
```

## Angular Change Detection Optimization

### OnPush Strategy in Components

```typescript
import { Component, OnInit, ChangeDetectionStrategy, Input } from '@angular/core';

@Component({
  selector: 'app-business-card',
  template: `
	<div class="business-card">
	  <img [src]="business.visitingCard" alt="Card">
	  <h3>{{ business.name }}</h3>
	  <p>{{ business.description }}</p>
	  <span>Rating: {{ business.averageRating | number: '1.1-1' }}</span>
	</div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BusinessCardComponent {
  @Input() business: any;
}
```

Benefits:
- Change detection only runs when inputs change
- Reduces CPU usage by 30-50%
- Requires OnPush strategy on child components

## Angular Signals (Angular 16+)

### State Management with Signals

```typescript
import { Injectable } from '@angular/core';
import { signal, computed, effect } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class BusinessStateService {
  // Signals for reactive state
  businesses = signal<Business[]>([]);
  selectedCategoryId = signal<number | null>(null);
  isLoading = signal(false);

  // Computed signals for derived state
  filteredBusinesses = computed(() => {
	const categoryId = this.selectedCategoryId();
	if (!categoryId) return this.businesses();
	return this.businesses().filter(b => b.categoryID === categoryId);
  });

  businessCount = computed(() => this.businesses().length);

  constructor(private api: BusinessService) {
	// Effect: Automatically fetch when category changes
	effect(() => {
	  const categoryId = this.selectedCategoryId();
	  if (categoryId) {
		this.loadBusinesses(categoryId);
	  }
	});
  }

  loadBusinesses(categoryId: number) {
	this.isLoading.set(true);
	this.api.searchBusinesses(categoryId).subscribe(
	  data => {
		this.businesses.set(data);
		this.isLoading.set(false);
	  }
	);
  }
}
```

### Using Signals in Components

```typescript
import { Component } from '@angular/core';
import { BusinessStateService } from '../../services/business-state.service';

@Component({
  selector: 'app-business-list',
  template: `
	<div *ngIf="state.isLoading()">Loading...</div>
	<div class="business-list">
	  <app-business-card 
		*ngFor="let business of state.filteredBusinesses()"
		[business]="business"
	  ></app-business-card>
	</div>
	<p>Total: {{ state.businessCount() }}</p>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BusinessListComponent {
  constructor(public state: BusinessStateService) {}
}
```

## Virtual Scrolling for Large Lists

```typescript
import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ScrollingModule } from '@angular/cdk/scrolling';

@Component({
  selector: 'app-business-search',
  template: `
	<cdk-virtual-scroll-viewport itemSize="200" class="businesses-viewport">
	  <app-business-card 
		*cdkVirtualFor="let business of businesses"
		[business]="business"
	  ></app-business-card>
	</cdk-virtual-scroll-viewport>
  `,
  styleUrls: ['./search.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BusinessSearchComponent implements OnInit {
  businesses: Business[] = [];

  constructor(private api: BusinessService) {}

  ngOnInit() {
	this.api.getBusinesses().subscribe(data => {
	  this.businesses = data;
	});
  }
}
```

CSS for scroll container:
```css
.businesses-viewport {
  height: 600px;
  width: 100%;
  border: 1px solid #ddd;
}
```

## Track Function for *ngFor

```typescript
// In component
trackByBusinessId(index: number, item: Business): number {
  return item.businessID;
}

// In template
<app-business-card 
  *ngFor="let business of businesses; trackBy: trackByBusinessId"
  [business]="business"
></app-business-card>
```

Benefits:
- Prevents unnecessary component destruction/creation
- Keeps form state between change detection cycles
- Improves performance by 20-30% for large lists

## HTTP Caching Interceptor

```typescript
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpResponse } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';

interface CacheEntry {
  response: HttpResponse<any>;
  timestamp: number;
}

@Injectable()
export class HttpCachingInterceptor implements HttpInterceptor {
  private cache = new Map<string, CacheEntry>();
  private readonly CACHE_TTL = 5 * 60 * 1000; // 5 minutes

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
	// Only cache GET requests
	if (req.method !== 'GET') {
	  return next.handle(req);
	}

	// Check cache
	const cached = this.getFromCache(req.url);
	if (cached) {
	  return of(cached.clone({ status: 200 }));
	}

	// Forward request and cache response
	return next.handle(req).pipe(
	  tap(event => {
		if (event instanceof HttpResponse) {
		  this.cache.set(req.url, { response: event.clone(), timestamp: Date.now() });
		}
	  })
	);
  }

  private getFromCache(url: string): HttpResponse<any> | null {
	const cached = this.cache.get(url);
	if (!cached) return null;

	// Check if cache expired
	if (Date.now() - cached.timestamp > this.CACHE_TTL) {
	  this.cache.delete(url);
	  return null;
	}

	return cached.response;
  }

  clearCache() {
	this.cache.clear();
  }
}
```

Register in app.module.ts:
```typescript
providers: [
  {
	provide: HTTP_INTERCEPTORS,
	useClass: HttpCachingInterceptor,
	multi: true
  }
]
```

## Bundle Analysis

### Generate bundle report
```bash
# Generate stats
ng build --prod --stats-json

# Analyze with webpack-bundle-analyzer
npx webpack-bundle-analyzer dist/BusinessNewEnvironment/stats.json
```

## Performance Checklist

- [ ] Lazy-load feature modules (auth, business, admin)
- [ ] Use OnPush change detection strategy
- [ ] Implement trackBy for *ngFor loops
- [ ] Use virtual scrolling for large lists
- [ ] Implement signals for state management
- [ ] Add HTTP caching interceptor
- [ ] Use @defer for below-the-fold components
- [ ] Tree-shake unused code (production build)
- [ ] Run lighthouse audit regularly
- [ ] Monitor bundle size changes

## Expected Performance Gains

- **Bundle Size**: 30-40% reduction with lazy-loading + tree-shaking
- **TTI (Time to Interactive)**: 50-60% faster with lazy routes
- **FCP (First Contentful Paint)**: 40-50% faster with app shell strategy
- **CPU Usage**: 30-50% reduction with OnPush change detection
- **Memory**: 20-30% reduction with proper cleanup

## References
- [Angular Performance Guide](https://angular.io/guide/angular-performance)
- [Angular Change Detection](https://angular.io/guide/change-detection-skipping-checks)
- [Angular Signals](https://angular.io/guide/signals)
- [Virtual Scrolling](https://material.angular.io/cdk/scrolling/overview)
