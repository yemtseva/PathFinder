
function animatePath(shortestPath) {
    for (let i = 0; i < shortestPath.length; i++) {
        setTimeout(() => {
            shortestPath[i].className = 'node path';

        }, 15 * i);
    }
}

