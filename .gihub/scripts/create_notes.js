module.exports = async ({core, currentVersion, releaseType, prJson}) => {
    try {
        const prList = JSON.parse(prJson);

        const lines = [];
        lines.push(`## ${bumpVersion(currentVersion, releaseType)}(${new Date().toISOString().split('T')[0]})`);

        const changes = prFilter(prList, pr => hasLabel(pr, "feature") || hasLabel(pr, "enhancement")).map(createPrRow);
        if (changes.length > 0) {
            lines.push(`### Changes`);
            lines.push(...changes);
        }

        const fixes = prFilter(prList, pr => hasLabel(pr, "bug fix")).map(createPrRow);
        if (fixes.length > 0) {
            lines.push(`### Fixes`);
            lines.push(...fixes);
        }

        const result = lines.join(`\n`)
        core.info(result);

        return result;
    }
    catch(e) {
        core.setFailed(e);
    }

    function prFilter(srcPrList, predicate) {
        const result = [];

        const numberSet = new Set();
        for (const pr of srcPrList) {
            if (numberSet.has(pr.number) || !predicate(pr)) {
                continue;
            }

            result.push(pr);
            numberSet.add(pr.number);
        }

        return result;
    }

    function createPrRow(pr) {
        return `* ${pr.title} by [**@${pr.author.name}**](https://github.com/${pr.author.login}) in [**#${pr.number}**](${pr.url})`
    }

    function hasLabel(obj, labelPattern) {
        for (const label of obj.labels) {
            if (label.name?.toLowerCase() === labelPattern) {
                return true;
            }
        }

        return false;
    }

    function bumpVersion(currentVersion, releaseType) {
        const version = !currentVersion.startsWith('v')
            ? currentVersion
            : !currentVersion.startsWith('v.')
                ? currentVersion.substring(1)
                : currentVersion.substring(2);

        const parts = version.split('.').map(Number);
        if (parts.length !== 3 || parts.some(isNaN)) {
            throw new Error('Invalid version');
        }

        switch (releaseType.toLowerCase()) {
            case 'major':
                parts[0] += 1;
                parts[1] = 0;
                parts[2] = 0;
                break;

            case 'minor':
                parts[1] += 1;
                parts[2] = 0;
                break;

            case 'patch':
                parts[2] += 1;
                break;

            default:
                throw new Error('Invalid releaseType');
        }

        return `${parts[0]}.${parts[1]}.${parts[2]}`;
    }
}