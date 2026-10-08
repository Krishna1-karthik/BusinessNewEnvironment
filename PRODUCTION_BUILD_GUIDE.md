# Production Build & Bundle Optimization Guide

## Backend (.NET 10) Production Build

### 1. Build Configuration

```bash
# Development build
dotnet build

# Production build
dotnet build --configuration Release

# Publish for deployment
dotnet publish --configuration Release -o ./publish
```

### 2. Production appsettings.json

Create `appsettings.Production.json`:

```json
{
  "Logging": {
	"LogLevel": {
	  "Default": "Warning",
	  "Microsoft": "Warning",
	  "System": "Warning"
	}
  },
  "ConnectionStrings": {
	"DefaultConnection": "Server=your-prod-server;Database=business;User Id=sa;Password=YourSecurePassword;",
	"Redis": "your-redis-connection-string"
  },
  "GoogleMaps": {
	"ApiKey": "YOUR_API_KEY"
  },
  "Jwt": {
	"Key": "your-very-long-secret-key-min-256-bits",
	"Issuer": "your-issuer",
	"Audience": "your-audience"
  },
  "AllowedHosts": "*"
}
```

### 3. Performance Configuration for Production

Update `Program.cs` for production:

```csharp
// Configure request logging only in development
if (!app.Environment.IsProduction())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Use Https redirection in production
if (app.Environment.IsProduction())
{
	app.UseHttpsRedirection();
	app.UseHsts(); // Add HSTS header
}

// Configure Redis caching for production
if (app.Environment.IsProduction())
{
	builder.Services.AddStackExchangeRedisCache(options =>
	{
		options.Configuration = builder.Configuration.GetConnectionString("Redis");
	});
}
```

### 4. Production Deployment Checklist

- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Enable HTTPS only
- [ ] Configure Redis for distributed caching
- [ ] Set secure database connection strings
- [ ] Use strong JWT signing keys
- [ ] Enable response compression
- [ ] Configure CORS for specific domains
- [ ] Enable logging to Application Insights
- [ ] Set up database backups
- [ ] Configure API rate limiting
- [ ] Enable security headers
- [ ] Run database migrations

---

## Frontend (Angular) Production Build

### 1. Production Build Command

```bash
# Standard production build
ng build --configuration production

# Build with optimization flags
ng build --configuration production --optimization --build-optimizer

# Build with bundle analysis
ng build --configuration production --stats-json
```

### 2. Angular Production Configuration

Update `angular.json`:

```json
{
  "$schema": "./node_modules/@angular/cli/lib/config/schema.json",
  "version": 1,
  "newProjectRoot": "projects",
  "projects": {
	"BusinessNewEnvironment": {
	  "architect": {
		"build": {
		  "configurations": {
			"production": {
			  "budgets": [
				{
				  "type": "initial",
				  "maximumWarning": "500kb",
				  "maximumError": "1mb"
				},
				{
				  "type": "anyComponentStyle",
				  "maximumWarning": "2kb",
				  "maximumError": "4kb"
				}
			  ],
			  "outputHashing": "all",
			  "optimization": true,
			  "buildOptimizer": true,
			  "sourceMap": false,
			  "namedChunks": false,
			  "aot": true,
			  "extractLicenses": true,
			  "vendorChunk": false,
			  "serviceWorker": true,
			  "ngswConfigPath": "ngsw-config.json"
			}
		  }
		}
	  }
	}
  }
}
```

### 3. Bundle Analysis

```bash
# Install webpack-bundle-analyzer
npm install --save-dev webpack-bundle-analyzer

# Generate stats file
ng build --configuration production --stats-json

# Analyze bundle
npx webpack-bundle-analyzer dist/BusinessNewEnvironment/stats.json
```

### 4. Expected Bundle Sizes

Before optimization:
- main.js: ~500KB
- vendor.js: ~800KB
- Total: ~1.5MB

After optimization:
- main.js: ~150-200KB (gzip: ~40-50KB)
- vendor.js: ~250-300KB (gzip: ~70-80KB)
- Total: ~400-500KB (gzip: ~120-150KB)

### 5. Code Splitting Configuration

In `angular.json`, enable lazy-loading:

```json
{
  "build": {
	"configurations": {
	  "production": {
		"lazyModuleMap": {
		  "features/business/business.module": ["BusinessModule"],
		  "features/admin/admin.module": ["AdminModule"],
		  "features/auth/auth.module": ["AuthModule"]
		}
	  }
	}
  }
}
```

### 6. Bundlesize Monitoring

Create `.bundlebudgetrc`:

```json
{
  "bundles": [
	{
	  "name": "main",
	  "maxSize": "150kb"
	},
	{
	  "name": "vendor",
	  "maxSize": "300kb"
	},
	{
	  "name": "business",
	  "maxSize": "100kb"
	},
	{
	  "name": "admin",
	  "maxSize": "80kb"
	}
  ]
}
```

### 7. Tree-Shaking Verification

Ensure these in `tsconfig.json`:

```json
{
  "compilerOptions": {
	"module": "es2020",
	"target": "es2020",
	"lib": ["es2020", "dom"],
	"declaration": false,
	"declarationMap": false,
	"sourceMap": false,
	"removeComments": true,
	"moduleResolution": "node"
  }
}
```

### 8. Remove Unused Dependencies

```bash
# Audit dependencies
npm audit

# Find unused dependencies
npx depcheck

# Remove unused packages
npm uninstall [package-name]
```

Common unused packages to check:
- Unused polyfills
- Debug libraries
- Demo/example code
- Old HTTP clients (if migrated to HttpClient)

### 9. Performance Monitoring Commands

```bash
# Run Lighthouse audit
npm run lighthouse

# Check build size
ng build --prod --stats-json && npx webpack-bundle-analyzer dist/stats.json

# Performance test
npm run e2e -- --performance
```

### 10. Deployment Optimization

**For GitHub Pages:**

```bash
# Build for GitHub Pages
ng build --configuration production --base-href "/BusinessNewEnvironment/"

# Deploy
npm run deploy
```

**For Docker:**

```dockerfile
# Multi-stage build
FROM node:18-alpine AS builder
WORKDIR /app
COPY . .
RUN npm ci
RUN ng build --configuration production

FROM nginx:alpine
COPY --from=builder /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

---

## Performance Optimization Checklist

### Backend Metrics
- [ ] Response compression: Gzip/Brotli enabled
- [ ] Database: Indexes created, queries optimized
- [ ] Caching: Redis configured, cache headers set
- [ ] Logging: Minimal in production
- [ ] Security: HTTPS, security headers, CORS configured
- [ ] Monitoring: Application Insights configured
- [ ] Rate limiting: Implemented
- [ ] Database connection pooling: Optimized

### Frontend Metrics
- [ ] Bundle size < 500KB (with compression < 150KB)
- [ ] Lighthouse score > 90
- [ ] Time to Interactive < 3 seconds
- [ ] First Contentful Paint < 1.5 seconds
- [ ] Largest Contentful Paint < 2.5 seconds
- [ ] Cumulative Layout Shift < 0.1
- [ ] Service Worker: Enabled and registered
- [ ] Lazy loading: Implemented for routes
- [ ] Tree-shaking: Verified
- [ ] Images: Optimized (WebP, lazy-loaded)

### Infrastructure
- [ ] CDN: Configured for static assets
- [ ] Caching: HTTP headers set correctly
- [ ] Compression: Enabled at server level
- [ ] HTTPS: Enforced, cert valid
- [ ] Monitoring: Real-time alerts configured
- [ ] Backups: Automated and tested
- [ ] Scaling: Load balancing configured
- [ ] DNS: CNAME/A records configured

---

## Expected Performance Gains (After All Optimizations)

### Page Load Metrics
| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| First Contentful Paint (FCP) | 4.2s | 1.8s | 57% faster |
| Largest Contentful Paint (LCP) | 6.1s | 2.3s | 62% faster |
| Time to Interactive (TTI) | 8.5s | 2.8s | 67% faster |
| Total Bundle Size | 1.8MB | 450KB | 75% smaller |
| Gzip Size | 420KB | 120KB | 71% smaller |

### What Users Experience
- ✅ Faster page load (57-67% improvement)
- ✅ Responsive UI with Signals (30-50% CPU reduction)
- ✅ Instant navigation with lazy loading
- ✅ Works offline with service worker
- ✅ Optimal images with lazy loading

### SEO Impact
- ✅ Higher Lighthouse scores (>90)
- ✅ Better Core Web Vitals
- ✅ Mobile-friendly performance
- ✅ Server-side rendering ready

---

## Continuous Monitoring

### Weekly Checks
- Review bundle size changes
- Check performance metrics
- Monitor error rates
- Audit security

### Monthly Reviews
- Dependency updates
- Performance regression testing
- Load testing
- Capacity planning

### Tools for Monitoring
- [Google PageSpeed Insights](https://pagespeed.web.dev/)
- [WebPageTest](https://www.webpagetest.org/)
- Chrome DevTools Performance tab
- Application Insights
- AWS CloudWatch / Azure Monitor

