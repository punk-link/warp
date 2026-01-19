import { ref, readonly } from 'vue'
import { NotifyLevel } from '../types/notify-level'
import type { NotificationItem } from '../types/notification-item'
import type { NotificationOptions } from '../types/notification-options'


const DEFAULT_TTL_INFO_MS = 3_500
const DEFAULT_TTL_WARN_MS = 6_000
const DEFAULT_TTL_ERROR_MS = 8_000


interface RateLimitConfig {
	maxPerWindow: number
	windowMs: number
}


interface NotificationsState {
	items: ReturnType<typeof ref<NotificationItem[]>>
	maxStack: number
	rateLimit: RateLimitConfig
	dedupeWindowMs: number
}


let singleton: {
	state: NotificationsState
	push: (options: NotificationOptions) => string | null
	remove: (id: string) => void
	clear: () => void
	setMaxStack: (n: number) => void
	setRateLimit: (cfg: RateLimitConfig) => void
	setDedupeWindowMs: (ms: number) => void
} | null = null


const removeTimers = new Map<string, number>()
const recentPushes: number[] = []


function ensure(): NonNullable<typeof singleton> {
	if (singleton)
		return singleton

	const items = ref<NotificationItem[]>([])

	const state: NotificationsState = {
		items,
		maxStack: 4,
		rateLimit: { maxPerWindow: 6, windowMs: 10_000 },
		dedupeWindowMs: 4_000
	}


	function scheduleRemoval(item: NotificationItem) {
		if (item.sticky || item.expiresAt == null)
			return

		const existing = removeTimers.get(item.id)
		if (existing)
			window.clearTimeout(existing)

		const delay = Math.max(0, item.expiresAt - Date.now())
		const handle = window.setTimeout(() => {
			remove(item.id)
		}, delay)

		removeTimers.set(item.id, handle)
	}


	function normalize(options: NotificationOptions): NotificationItem {
		const now = Date.now()

		const defaultTtl = options.sticky ? 0 : (
			options.ttlMs != null ? options.ttlMs : (
				options.level === NotifyLevel.Info ? DEFAULT_TTL_INFO_MS :
					options.level === NotifyLevel.Warn ? DEFAULT_TTL_WARN_MS :
						DEFAULT_TTL_ERROR_MS
			)
		)

		const ttlMs = Math.max(0, defaultTtl)
		const sticky = options.sticky === true || ttlMs === 0
		const expiresAt = sticky ? null : now + ttlMs

		const item: NotificationItem = {
			id: `${now}-${Math.random().toString(36).slice(2, 8)}`,
			level: options.level,
			title: options.title,
			message: options.message ?? '',
			details: options.details,
			actions: options.actions,
			dedupeKey: options.dedupeKey,
			sticky,
			ttlMs,
			createdAt: now,
			expiresAt,
			occurrences: 1
		}

		return item
	}


	function withinWindow(ts: number, windowMs: number): boolean {
		return Date.now() - ts <= windowMs
	}


	function recordPush() {
		const now = Date.now()

		while (recentPushes.length && !withinWindow(recentPushes[0], state.rateLimit.windowMs))
			recentPushes.shift()

		recentPushes.push(now)
	}

	function isRateLimited(): boolean {
		const now = Date.now()

		while (recentPushes.length && now - recentPushes[0] > state.rateLimit.windowMs)
			recentPushes.shift()

		return recentPushes.length >= state.rateLimit.maxPerWindow
	}

	function push(options: NotificationOptions): string | null {
		const now = Date.now()

		if (isRateLimited()) {
			if (options.dedupeKey) {
				const idx = items.value.findIndex(n => n.dedupeKey === options.dedupeKey && (now - n.createdAt) <= state.dedupeWindowMs)
				if (idx >= 0) {
					const existing = items.value[idx]
					existing.occurrences++
					existing.createdAt = now
					if (!existing.sticky)
						existing.expiresAt = now + existing.ttlMs

					scheduleRemoval(existing)

					return existing.id
				}
			}

			return null
		}

		if (options.dedupeKey) {
			const idx = items.value.findIndex(n => n.dedupeKey === options.dedupeKey && (now - n.createdAt) <= state.dedupeWindowMs)
			if (idx >= 0) {
				const existing = items.value[idx]
				existing.occurrences++
				existing.createdAt = now
				if (!existing.sticky)
					existing.expiresAt = now + existing.ttlMs

				scheduleRemoval(existing)
				recordPush()

				return existing.id
			}
		}

		const item = normalize(options)

		if (state.maxStack > 0 && items.value.length >= state.maxStack) {
			const idx = items.value.findIndex(n => !n.sticky)
			if (idx >= 0)
				items.value.splice(idx, 1)
			else
				items.value.shift()
		}

		items.value.push(item)
		scheduleRemoval(item)
		recordPush()

		return item.id
	}


	function remove(id: string) {
		const i = items.value.findIndex(n => n.id === id)
		if (i >= 0)
			items.value.splice(i, 1)

		const handle = removeTimers.get(id)
		if (handle)
			window.clearTimeout(handle)

		removeTimers.delete(id)
	}


	function clear() {
		for (const id of Array.from(removeTimers.keys())) {
			const handle = removeTimers.get(id)
			if (handle)
				window.clearTimeout(handle)
		}

		removeTimers.clear()
		items.value = []
	}


	function setMaxStack(n: number) {
		state.maxStack = Math.max(0, Math.floor(n))
	}


	function setRateLimit(cfg: RateLimitConfig) {
		state.rateLimit = { maxPerWindow: Math.max(1, Math.floor(cfg.maxPerWindow)), windowMs: Math.max(500, Math.floor(cfg.windowMs)) }
	}


	function setDedupeWindowMs(ms: number) {
		state.dedupeWindowMs = Math.max(0, Math.floor(ms))
	}


	singleton = {
		state,
		push,
		remove,
		clear,
		setMaxStack,
		setRateLimit,
		setDedupeWindowMs
	}

	return singleton
}


export function useNotifications() {
	const { state, push, remove, clear, setMaxStack, setRateLimit, setDedupeWindowMs } = ensure()

	return {
		items: readonly(state.items),
		push,
		remove,
		clear,
		setMaxStack,
		setRateLimit,
		setDedupeWindowMs
	}
}
