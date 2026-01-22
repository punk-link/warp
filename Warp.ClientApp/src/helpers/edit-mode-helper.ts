import { EditMode } from "../types/entries/enums/edit-modes";


/** Parses a value into an EditMode, defaulting to EditMode.Simple if invalid. */
export function parseEditMode(value: unknown): EditMode {
	return (typeof value === 'string' && (Object.values(EditMode) as string[]).includes(value))
		? (value as EditMode)
		: EditMode.Simple
}