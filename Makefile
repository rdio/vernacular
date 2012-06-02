PREFIX := /usr/local
LIBDIR := $(PREFIX)/lib
BINDIR := $(PREFIX)/bin

INSTALL_FILES = \
	Mono.Cecil.dll \
	Mono.Cecil.Mdb.dll \
	Mono.Cecil.Pdb.dll \
	Vernacular.Catalog.dll \
	Vernacular.Catalog.dll.mdb \
	Vernacular.Potato.dll \
	Vernacular.Potato.dll.mdb \
	Vernacular.exe \
	Vernacular.exe.mdb

all:
	xbuild Vernacular.sln

install:
	mkdir -p "$(LIBDIR)/vernacular"
	mkdir -p "$(BINDIR)"
	for file in $(INSTALL_FILES); do \
		install -m 0755 "Vernacular.Tool/bin/Debug/$$file" "$(LIBDIR)/vernacular"; \
	done
	sed 's|@libdir@|$(LIBDIR)|' < vernacular.in > "$(BINDIR)/vernacular"
	chmod 0755 "$(BINDIR)/vernacular"
