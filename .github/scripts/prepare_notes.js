module.exports = async ({core, fs, lastTag}) => {
    try {
        const changelog = fs.readFileSync('CHANGELOG.md', 'utf8');

        const matches = [...changelog.matchAll("## ([0-9].[0-9].[0-9])")];
        if (!matches) {
            throw new Error("empty match");
        }

        const version = matches[0][1];
        if (`v${version}` === lastTag) {
            throw new Error(`The version(${notesLength}) in the notes is the same as in the tag(${lastTag})`);
        }

        const notesLength = matches.length < 2
            ? undefined
            : matches[1].index - matches[0].index;

        const notes = changelog.substring(matches[0].index, notesLength)
            .split("\n")
            .slice(1)
            .join("\n")

        if (notes.trim().length === 0) {
            throw new Error(`The version(${notesLength}) has empty notes`);
        }

        const notesFile = 'release-notes.md';
        fs.writeFileSync(notesFile, notes);

        core.info(version);
        core.info(notes);

        core.setOutput('version', version);
        core.setOutput('notes', notesFile);
    }
    catch(e) {
        core.setFailed(e);
    }
}