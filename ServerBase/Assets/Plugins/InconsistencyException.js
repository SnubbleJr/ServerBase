#pragma strict

/**
 * Thrown if a network data inconsistency appears.
 * @author Chris
 */

class InconsistencyException extends UnityException {
	public function InconsistencyException(msg : String) {
		super(msg);
	}
}