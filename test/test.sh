#!/usr/bin/env bash

../bin/Debug/Vernacular.exe \
	-g po \
	--reduce-master plurals.po \
	--reduce-retain plurals.pot > .plurals-merged-test.po
diff -u plurals-merged.po .plurals-merged-test.po
rm .plurals-merged-test.po

echo "Done. No diff should be displayed."
