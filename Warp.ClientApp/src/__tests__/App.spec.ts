import { render, screen } from '@testing-library/vue'
import { describe, it, expect } from 'vitest'
import App from '../App.vue'

// Minimal smoke test to ensure the app renders base layout

describe('App.vue', () => {
  it('renders heading', async () => {
    render(App)
    const title = await screen.findByRole('heading', { level: 1 })
    expect(title.textContent?.toLowerCase()).toContain('warp')
  })
})
