# FakeDataEngine

Sometimes working with database, you need to continuously insert records in a table, e.g testing a CDC implementation, with near real data.  FakeDataEngine tries to solves that problem. FakeDataEngine is inspired in [volube](https://github.com/MichaelDrogalis/voluble).

* Entirely developed using Net Core 3.1
* FakeDataEngine uses [Bogus](https://github.com/bchavez/Bogus) to generate fake data.
* Sql Server is the only database supported until now.

## Running with Docker

Images are hosted at [Docker Hub](https://hub.docker.com/repository/docker/fglaeser/fakedataengine).

Launch container:

```sh
docker run -it --rm \
    -e FAKER_CONFIG_PATH=/opt/config.yml \
    -v ./config.yml:/opt/config.yml \
    fglaeser/fakedataengine:0.1
    
```

or using a `docker-compose.yml` file to launch the container:

```yaml

services:
  faker:
    image: fglaeser/fakedataengine:0.1
    environment:
      FAKER_CONFIG_PATH: /opt/config.yml
    volumes:
      - ./config.yml:/opt/config.yml

```
Then:

```sh
docker-compose up
```

## Configuration

Let's check a `config.yml`, you could check that the configuration is pretty straightforward:

```yaml

connection.string: "Data Source=mssql;Initial Catalog=DemoDB;Persist Security Info=True;User ID=sa;Password=passw0rd!;"
throttle.ms: 5000
tables:
  - name: Employee
    schema: dbo
    columns:
    - name: ID
      value: "{{randomizer.number(1,1000)}}"
    - name: Name
      value: "{{name.firstname(Male)}}"
    - name: Salary
      value: "{{randomizer.number(1,1000)}}"
    - name: payload
      format: json
      object:
        id: "{{randomizer.number(1,1000)}}"
        name: "{{name.firstname(Female)}}"
        mode: 1

```

You can configure your database with the `connection.string` property and how fast the data is generated with `throttle.ms`.
### Tables
To configure tables you need to set the following properties:

* `name`: Table name.
* `schema`: Table schema.
* `columns`: Array of columns you want to include in the insert statement.

### Columns
To configure a column you need to set the following properties:

* `name`: Column name.
* `format`: Default value is `raw`, but you can also set this value to `json` in order to fill the column with a valid json string.
* `value`: Here you can use a fix value or a [Bogus handlebar](https://github.com/bchavez/Bogus#parse-handlebars)
* `object`: If you column format is `json`, use this to define the properties of your json object.

In the previous example, the column payload will be fill with a json string like the following:

```json
{"id": "890", "name": "Anna", "mode": 1 }
```


