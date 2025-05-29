import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { SyncfusionLicenseModule } from '@syncfusion/ej2-angular-base';

@NgModule({
  declarations: [], // Should be empty
  imports: [
    HttpClientModule,
    RouterModule.forRoot([]), // Or ensure routes are in app.routes.ts and provided in app.config.ts
    SyncfusionLicenseModule
  ],
  providers: [], // Global services might be here or in app.config.ts
  // bootstrap: [] // Should be empty or not present
})
export class AppModule {
  constructor() {
    SyncfusionLicenseModule.setLicenseKey('Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXpceXVQRGNcVEN+V0dWYUA=');
  }
}
