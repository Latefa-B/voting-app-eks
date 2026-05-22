var express = require('express'),
    async   = require('async'),
    { Pool } = require('pg'),
    path    = require('path'),
    app     = express();

var port = process.env.PORT || 4000;

app.use(express.static(__dirname + '/views'));
app.use(express.json());

var pool = new Pool({
  connectionString: 'postgres://postgres:postgres@db/votes'
});

async function getVotes() {
  const client = await pool.connect();
  try {
    const result = await client.query(
      'SELECT vote, COUNT(id) AS count FROM votes GROUP BY vote'
    );
    const votes = { a: 0, b: 0 };
    result.rows.forEach(row => {
      votes[row.vote] = parseInt(row.count);
    });
    return votes;
  } finally {
    client.release();
  }
}

app.get('/', function(req, res) {
  res.sendFile(path.resolve(__dirname + '/views/index.html'));
});

app.get('/votes', async function(req, res) {
  try {
    const votes = await getVotes();
    res.json(votes);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Database not ready yet' });
  }
});

app.listen(port, function() {
  console.log('Result app listening on port ' + port);
});
