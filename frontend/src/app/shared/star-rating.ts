import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-stars',
  template: `<span class="stars" [title]="value() + ' / 5'">{{ text() }}</span>`,
  styles: `.stars { color: #f5a623; letter-spacing: 1px; white-space: nowrap; }`
})
export class StarRating {
  value = input.required<number>();
  text = computed(() => {
    const full = Math.round(this.value());
    return '★'.repeat(full) + '☆'.repeat(5 - full);
  });
}
