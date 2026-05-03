// 'import' — ES module keyword; brings named exports from another module into this file's scope
// 'Component' — Angular decorator factory that marks a class as a component and attaches metadata
import { Component } from '@angular/core';
// 'CommonModule' — Angular module that provides *ngIf, *ngFor, async pipe, and other common directives
import { CommonModule } from '@angular/common';
// 'RouterLink' — Angular directive that turns an anchor tag into a client-side navigation link
import { RouterLink } from '@angular/router';

/**
 * Landing page component.
 * Displays the hero section, how-it-works steps, cuisine categories,
 * platform benefits, partner CTAs, and a final call-to-action banner.
 */
// '@Component' — class decorator; Angular reads this metadata to compile the template and wire up DI
@Component({
  selector: 'app-landing',  // CSS selector used in templates/router to render this component: <app-landing>
  standalone: true,         // standalone: true — no NgModule needed; the component manages its own imports
  imports: [CommonModule, RouterLink], // 'imports' — declares Angular dependencies available inside this component's template
  template: `
    <!-- ── HERO ── -->
    <section class="hero">
      <div class="hero-bg"></div>
      <div class="hero-inner">
        <div class="hero-text">
          <div class="hero-tag">🔥 India's fastest food delivery</div>
          <h1>
            Satisfy Every<br>
            <span class="gradient-text">Craving, Fast.</span>
          </h1>
          <p class="hero-sub">
            From biryani to burgers, sushi to shawarma — your next favourite meal is just a tap away. Order from the best restaurants near you.
          </p>
          <div class="hero-cta">
            <a routerLink="/restaurants" class="btn-hero-primary">
              🍴 Order Now
            </a>
            <a routerLink="/auth/register" class="btn-hero-outline">
              List Your Restaurant →
            </a>
          </div>
          <div class="hero-stats">
            <div class="stat"><span class="stat-num">500+</span><span class="stat-lbl">Restaurants</span></div>
            <div class="stat-div"></div>
            <div class="stat"><span class="stat-num">~25 min</span><span class="stat-lbl">Avg Delivery</span></div>
            <div class="stat-div"></div>
            <div class="stat"><span class="stat-num">4.8 ★</span><span class="stat-lbl">Avg Rating</span></div>
          </div>
        </div>
        <div class="hero-visual">
          <div class="food-float food-1">🍕</div>
          <div class="food-float food-2">🍔</div>
          <div class="food-float food-3">🍜</div>
          <div class="food-float food-4">🌮</div>
          <div class="food-float food-5">🍣</div>
          <div class="food-float food-6">🍰</div>
          <div class="hero-circle">
            <div class="circle-inner">
              <span class="big-emoji">🛒</span>
              <span class="circle-label">CraveKart</span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ── HOW IT WORKS ── -->
    <section class="section how-it-works">
      <div class="section-inner">
        <div class="section-tag">Simple & Fast</div>
        <h2 class="section-title">Order in <span class="highlight">3 easy steps</span></h2>
        <div class="steps-grid">
          <div class="step-card">
            <div class="step-icon">📍</div>
            <div class="step-num">01</div>
            <h3>Choose Your Location</h3>
            <p>Enter your delivery address and discover restaurants near you.</p>
          </div>
          <div class="step-arrow">→</div>
          <div class="step-card">
            <div class="step-icon">🍽️</div>
            <div class="step-num">02</div>
            <h3>Pick Your Meal</h3>
            <p>Browse menus, add items to your cart, and customise your order.</p>
          </div>
          <div class="step-arrow">→</div>
          <div class="step-card">
            <div class="step-icon">🚴</div>
            <div class="step-num">03</div>
            <h3>Fast Delivery</h3>
            <p>Track your order in real-time as our riders bring it to your door.</p>
          </div>
        </div>
      </div>
    </section>

    <!-- ── CATEGORIES ── -->
    <section class="section categories">
      <div class="section-inner">
        <div class="section-tag">What's on the menu</div>
        <h2 class="section-title">Popular <span class="highlight">Categories</span></h2>
        <div class="cat-grid">
          <a routerLink="/restaurants" class="cat-card" *ngFor="let c of categories">
            <div class="cat-emoji">{{ c.emoji }}</div>
            <div class="cat-name">{{ c.name }}</div>
          </a>
        </div>
      </div>
    </section>

    <!-- ── WHY CRAVEKART ── -->
    <section class="section why">
      <div class="section-inner">
        <div class="section-tag">Why choose us</div>
        <h2 class="section-title">Built for <span class="highlight">food lovers</span></h2>
        <div class="why-grid">
          <div class="why-card" *ngFor="let w of whys">
            <div class="why-icon">{{ w.icon }}</div>
            <h3>{{ w.title }}</h3>
            <p>{{ w.desc }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- ── FOR PARTNERS ── -->
    <section class="section partners">
      <div class="section-inner partners-inner">
        <div class="partner-block">
          <div class="partner-emoji">🏪</div>
          <h3>Own a Restaurant?</h3>
          <p>Join thousands of restaurant owners growing their business on CraveKart. Easy setup, real-time orders, and powerful analytics.</p>
          <a routerLink="/auth/register" class="btn-partner">Partner With Us →</a>
        </div>
        <div class="partner-divider"></div>
        <div class="partner-block">
          <div class="partner-emoji">🚴</div>
          <h3>Want to Deliver?</h3>
          <p>Earn on your own schedule. Register as a delivery agent and start earning with every order you complete.</p>
          <a routerLink="/auth/register" class="btn-partner">Start Delivering →</a>
        </div>
      </div>
    </section>

    <!-- ── CTA BANNER ── -->
    <section class="cta-banner">
      <div class="cta-inner">
        <h2>Ready to satisfy your cravings?</h2>
        <p>Join over 50,000 happy customers ordering on CraveKart every day.</p>
        <a routerLink="/restaurants" class="btn-hero-primary large">
          🍴 Explore Restaurants
        </a>
      </div>
    </section>
  `,
  styleUrl: './landing.scss'
})
// 'export' — makes this class importable by the Angular router and other modules
// 'class'  — ES6/TypeScript keyword; defines a reusable blueprint with properties and methods
export class LandingComponent {
  // Array literal — an ordered, indexed collection of objects; TypeScript infers the element type from the values
  categories = [
    { emoji: '🍕', name: 'Pizza' },      // object literal — anonymous inline object with two string properties
    { emoji: '🍔', name: 'Burgers' },
    { emoji: '🍛', name: 'Biryani' },
    { emoji: '🍣', name: 'Sushi' },
    { emoji: '🌮', name: 'Mexican' },
    { emoji: '🍜', name: 'Chinese' },
    { emoji: '🥗', name: 'Healthy' },
    { emoji: '🍰', name: 'Desserts' },
    { emoji: '☕', name: 'Cafe' },
    { emoji: '🥙', name: 'Kebabs' },
    { emoji: '🍝', name: 'Italian' },
    { emoji: '🦞', name: 'Seafood' },
  ];

  // Array of objects — each element is an anonymous object with three string properties
  whys = [
    { icon: '⚡', title: 'Lightning Fast', desc: 'Average delivery time of 25 minutes. We know you\'re hungry.' },
    { icon: '🔒', title: 'Safe & Secure', desc: 'Your payments and personal data are always protected.' },
    { icon: '📍', title: 'Live Tracking', desc: 'Watch your order move in real-time from kitchen to your door.' },
    { icon: '⭐', title: 'Top Rated', desc: 'Only the best restaurants make it onto CraveKart.' },
    { icon: '💳', title: 'Easy Payments', desc: 'Pay by card or cash on delivery — your choice.' },
    { icon: '🎯', title: 'Personalised', desc: 'Smart recommendations based on what you love to eat.' },
  ];
}
