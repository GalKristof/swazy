import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-swazy-platform',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './swazy-platform.html',
  styleUrls: ['./swazy-platform.scss']
})
export class SwazyPlatformComponent {
  openSignupModal() {
    const modal = document.getElementById('signup_modal') as HTMLDialogElement;
    modal?.showModal();
  }

  scrollToSection(sectionId: string) {
    const element = document.getElementById(sectionId);
    element?.scrollIntoView({ behavior: 'smooth' });
  }
}
