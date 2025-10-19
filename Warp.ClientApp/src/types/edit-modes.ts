export enum EditMode {
	Unset = 'Unset',
	Simple = 'Simple',
	Advanced = 'Advanced'
}


export function parseEditMode(value: unknown): EditMode {
	return (typeof value === 'string' && (Object.values(EditMode) as string[]).includes(value))
		? (value as EditMode)
		: EditMode.Simple
}
