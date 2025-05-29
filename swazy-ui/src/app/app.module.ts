import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http'; // Import HttpClientModule
import { SyncfusionLicenseModule } from '@syncfusion/ej2-angular-base';
import { CalendarModule } from '@syncfusion/ej2-angular-calendars';

import { AppComponent } from './app.component';
import { UiTestComponent } from './ui-test/ui-test.component';

@NgModule({
  declarations: [
    AppComponent,
    UiTestComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule, // Add HttpClientModule here
    SyncfusionLicenseModule,
    CalendarModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {
  constructor() {
    SyncfusionLicenseModule.setLicenseKey('Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXpceXVQRGNcVEN+V0dWYUA=');
  }
}
