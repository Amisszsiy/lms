import { RenderMode, ServerRoute } from '@angular/ssr';

// Every page depends on the login cookie, so render in the browser only.
export const serverRoutes: ServerRoute[] = [
  {
    path: '**',
    renderMode: RenderMode.Client
  }
];
