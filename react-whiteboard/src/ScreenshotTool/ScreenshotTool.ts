import { StateNode, TLCancelEvent, TLInterruptEvent } from '@tldraw/tldraw'
import { exportAs } from '@tldraw/tldraw'

export class ScreenshotTool extends StateNode {

	static override id = 'screenshot'

	override onEnter = () => {
		const shapes = this.editor.getCurrentPageShapeIds()
		if (shapes.size !== 0) {
			exportAs(this.editor, this.editor.getSelectedShapeIds())
		}
		this.complete()
	}

	override onExit = () => {
		
	}

	override onInterrupt: TLInterruptEvent = () => {
		this.complete()
	}

	override onCancel: TLCancelEvent = () => {
		this.complete()
	}

	private complete() {
		this.parent.transition('select', {})
	}
}
