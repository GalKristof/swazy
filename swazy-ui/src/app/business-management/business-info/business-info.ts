import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Business } from '../../models/business';

@Component({
  selector: 'app-business-info',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './business-info.html'
})
export class BusinessInfoComponent {
  business = input.required<Business | null>();

  businessUpdated = output<Business>();

  isEditing = false;
  isSaving = false;
  editForm: Business | null = null;

  businessTypes = [
    'BarberSalon',
    'HairSalon',
    'MassageSalon',
    'BeautySalon',
    'NailSalon',
    'Trainer',
    'ServiceProvider',
    'Restaurant',
    'Other'
  ];

  daisyThemes = [
    'light',
    'dark',
    'cupcake',
    'bumblebee',
    'emerald',
    'corporate',
    'synthwave',
    'retro',
    'cyberpunk',
    'valentine',
    'halloween',
    'garden',
    'forest',
    'aqua',
    'lofi',
    'pastel',
    'fantasy',
    'wireframe',
    'black',
    'luxury',
    'dracula',
    'cmyk',
    'autumn',
    'business',
    'acid',
    'lemonade',
    'night',
    'coffee',
    'winter',
    'dim',
    'nord',
    'sunset'
  ];

  startEditing() {
    if (this.business()) {
      this.editForm = { ...this.business() } as Business;
      this.isEditing = true;
    }
  }

  cancelEditing() {
    this.isEditing = false;
    this.editForm = null;
  }

  save() {
    if (this.editForm && !this.isSaving) {
      this.isSaving = true;
      this.businessUpdated.emit(this.editForm);
    }
  }

  onSaveComplete() {
    this.isSaving = false;
    this.isEditing = false;
    this.editForm = null;
  }

  onSaveError() {
    this.isSaving = false;
  }

  onThemeChange(theme: string) {
    if (this.editForm) {
      this.editForm.theme = theme;
      this.applyTheme(theme);
    }
  }

  private applyTheme(theme: string) {
    if (theme) {
      document.documentElement.setAttribute('data-theme', theme);
    }
  }
}
