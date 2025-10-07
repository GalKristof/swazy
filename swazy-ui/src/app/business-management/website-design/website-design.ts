import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Business } from '../../models/business';

@Component({
  selector: 'app-website-design',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './website-design.html'
})
export class WebsiteDesignComponent {
  business = input.required<Business | null>();
  businessUpdated = output<Business>();

  isEditing = false;
  isSaving = false;
  editForm: Business | null = null;

  daisyThemes = [
    'light', 'dark', 'cupcake', 'bumblebee', 'emerald', 'corporate', 'synthwave',
    'retro', 'cyberpunk', 'valentine', 'halloween', 'garden', 'forest', 'aqua',
    'lofi', 'pastel', 'fantasy', 'wireframe', 'black', 'luxury', 'dracula',
    'cmyk', 'autumn', 'business', 'acid', 'lemonade', 'night', 'coffee', 'winter',
    'dim', 'nord', 'sunset'
  ];

  startEditing() {
    this.editForm = { ...this.business()! };
    this.isEditing = true;
  }

  cancelEditing() {
    this.editForm = null;
    this.isEditing = false;
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
    document.documentElement.setAttribute('data-theme', theme);
  }
}
