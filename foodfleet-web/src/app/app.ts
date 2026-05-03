import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/navbar/navbar';
import { FooterComponent } from './shared/footer/footer';

/**
 * Root application component.
 * Renders the global navbar, the router outlet for page content, and the footer.
 */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, FooterComponent],
  templateUrl: './app.html',
  styles: [`
    :host { display: flex; flex-direction: column; min-height: 100vh; background: var(--bg); }
  `]
})
export class App {}
