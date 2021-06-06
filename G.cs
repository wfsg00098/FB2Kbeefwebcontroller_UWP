using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;

namespace FB2Kbeefwebcontroller_UWP
{
    static class G
    {
        public const int ORDER_DEFAULT = 0;
        public const int ORDER_TITLE_ASC = 1;
        public const int ORDER_TITLE_DESC = 2;
        public const int ORDER_ARTIST_ASC = 3;
        public const int ORDER_ARTIST_DESC = 4;
        public const int ORDER_ALBUM_ASC = 5;
        public const int ORDER_ALBUM_DESC = 6;
        public const int ORDER_RANDOM = 7;

        public static string db_name = "config.db";
        public static SQLiteConnection db;

        public static int Server_id;
        public static string Server_addr;
        public static string Server_user;
        public static string Server_pass;

        public static string current_playing_song_playlist;
        public static int current_playing_song_id;

        public static string current_playlist;
        public static int current_playlist_order;

        public static int tracks_limit;

        public static bool accurate_search = false;

        public static bool changed_frame = false;

        public static bool updateing_playlist = false;
        public static void print(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }
        public static string get_auth_phrase()
        {
            string phrase = Server_user + ":" + Server_pass;
            string result = Convert.ToBase64String(Encoding.Default.GetBytes(phrase), Base64FormattingOptions.None);
            return result;
        }

        public static int db_get_current_server()
        {
            string sql = "select server from current";
            ISQLiteStatement stat = db.Prepare(sql);
            SQLiteResult result = stat.Step();
            int id;
            if (SQLiteResult.ROW == result)
            {
                id = (int)stat.GetInteger(0);
            } else
            {
                id = 0;
            }
            stat.Dispose();
            return id;
        }

        public static void db_select_server(int id)
        {
            string sql = "update current set server=" + id;
            ISQLiteStatement stat = db.Prepare(sql);
            stat.Step();
            sql = "select addr,user,pass from server where id=" + id;
            stat = db.Prepare(sql);
            SQLiteResult result = stat.Step();
            if (SQLiteResult.ROW == result)
            {
                Server_id = id;
                Server_addr = stat[0] as string;
                Server_user = stat[1] as string;
                Server_pass = stat[2] as string;
            }
            stat.Dispose();
        }

        public static int db_get_playlist_order()
        {
            int mode;
            string sql = "select mode from current";
            ISQLiteStatement stat = db.Prepare(sql);
            SQLiteResult result = stat.Step();
            if (SQLiteResult.ROW == result)
            {
                mode = (int)stat.GetInteger(0);
            } else
            {
                mode = 0;
            }
            stat.Dispose();
            return mode;
        }
        
        public static string[] db_get_playlists()
        {
            string[] lists = new string[] { "p1" };
            try
            {
                string sql = "select count(distinct list) from list_s" + Server_id;
                ISQLiteStatement stat = db.Prepare(sql);
                SQLiteResult result = stat.Step();
                int count;
                if (SQLiteResult.ROW == result)
                {
                    count = (int)stat.GetInteger(0);
                }
                else
                {
                    count = 0;
                }
                stat.Dispose();
                if (count > 0)
                {
                    sql = "select distinct list from list_s" + Server_id;
                    stat = db.Prepare(sql);
                    result = stat.Step();
                    lists = new string[count];
                    for (int i = 0; i < count; i++)
                    {
                        lists[i] = stat[0] as string;
                        stat.Step();
                    }
                    stat.Dispose();

                }
            } catch (Exception ex)
            {
                print(ex.HResult.ToString());
            }
            return lists;
        }

        public static string db_get_current_playlist()
        {
            string list;
            string sql = "select playlist from current";
            ISQLiteStatement stat = db.Prepare(sql);
            SQLiteResult result = stat.Step();
            if (SQLiteResult.ROW == result)
            {
                list = stat[0] as string;
            }
            else
            {
                list = db_get_playlists()[0];
            }
            stat.Dispose();
            return list;
        }

        public static int db_get_list_limit()
        {
            string sql = "select listlimit from current";
            ISQLiteStatement stat = db.Prepare(sql);
            SQLiteResult result = stat.Step();
            int order;
            if (SQLiteResult.ROW == result)
            {
                order = (int)stat.GetInteger(0);
            }
            else
            {
                order = 0;
            }
            stat.Dispose();
            return order;
        }

        public static JsonArray db_get_servers()
        {
            try
            {
                JsonArray ja = new JsonArray();
                string sql = "select * from server";
                ISQLiteStatement stat = db.Prepare(sql);
                SQLiteResult result = stat.Step();
                while(SQLiteResult.ROW == result)
                {
                    JsonObject json = new JsonObject();
                    json.SetNamedValue("id", JsonValue.CreateNumberValue((int)stat.GetInteger(0)));
                    json.SetNamedValue("addr", JsonValue.CreateStringValue(stat[1] as string));
                    json.SetNamedValue("user", JsonValue.CreateStringValue(stat[2] as string));
                    json.SetNamedValue("pass", JsonValue.CreateStringValue(stat[3] as string));
                    ja.Add(json);
                    result = stat.Step();
                }
                stat.Dispose();
                return ja;
            }catch (Exception ex)
            {
                print(ex.HResult.ToString());
                return null;
            }
            
        }

        public static string[] db_get_server(int id)
        {
            string[] server = new string[3];
            string sql = "select addr,user,pass from server where id=" + id.ToString();
            ISQLiteStatement stat = db.Prepare(sql);
            SQLiteResult result = stat.Step();
            if (SQLiteResult.ROW == result)
            {
                server[0] = stat[0] as string;
                server[1] = stat[1] as string;
                server[2] = stat[2] as string;
            }
            stat.Dispose();
            return server;
        }

        public static int db_add_server()
        {
            string sql = "insert into server(addr,user,pass) values('','','')";
            ISQLiteStatement stat = db.Prepare(sql);
            stat.Step();
            stat.Dispose();
            int id = 1;
            sql = "select max(id) from server";
            stat = db.Prepare(sql);
            SQLiteResult result = stat.Step();
            if (SQLiteResult.ROW == result)
            {
                id = (int)stat.GetInteger(0);
            }
            stat.Dispose();
            sql = "create table list_s" + id + " (list text, id int, title text, artist text, album text, length text)";
            stat = db.Prepare(sql);
            stat.Step();
            stat.Dispose();
            return id;
        }

        public static void db_del_server(int id)
        {
            string sql = "drop table list_s" + id.ToString();
            ISQLiteStatement stat = db.Prepare(sql);
            stat.Step();
            stat.Dispose();
            sql = "delete from server where id=" + id.ToString();
            stat = db.Prepare(sql);
            stat.Step();
            stat.Dispose();
        }

        public static void db_save_server(int id, string addr, string user, string pass)
        {
            string sql = "update server set addr='" + addr + "',user='" + user + "',pass='" + pass + "' where id=" + id.ToString();
            ISQLiteStatement stat = db.Prepare(sql);
            stat.Step();
            stat.Dispose();
        }

        public static void db_set_playlist_order(int mode)
        {
            string sql = "update current set mode=" + mode.ToString();
            ISQLiteStatement stat = db.Prepare(sql);
            stat.Step();
            stat.Dispose();
            current_playlist_order = mode;
        }

        public static void db_set_list_limit(int limit)
        {
            string sql = "update current set listlimit=" + limit;
            ISQLiteStatement stat = db.Prepare(sql);
            stat.Step();
            stat.Dispose();
        }

        public static string db_get_search_string()
        {
            string search = "";
            string sql = "select search from current";
            ISQLiteStatement stat = db.Prepare(sql);
            SQLiteResult result = stat.Step();
            if (SQLiteResult.ROW == result)
            {
                search = stat[0] as string;
            }
            stat.Dispose();

            return search;
        }

        public static void db_set_search_string(string search)
        {
            search = search.Replace("'", "''");
            string sql = "update current set search='" + search + "'";
            ISQLiteStatement stat = db.Prepare(sql);
            stat.Step();
            stat.Dispose();
        }

        public async static Task<bool> db_update_playlists()
        {
            try
            {
                string sql = "delete from list_s" + Server_id.ToString();
                ISQLiteStatement stat = db.Prepare(sql);
                stat.Step();
                stat.Dispose();
                HttpClient http = new HttpClient();
                string url = Server_addr + "/api/playlists";
                var headers = http.DefaultRequestHeaders;
                headers.TryAdd("Authorization", "Basic " + get_auth_phrase());
                HttpResponseMessage response = await http.GetAsync(new Uri(url));
                response.EnsureSuccessStatusCode();
                string body = await response.Content.ReadAsStringAsync();
                JsonObject json = JsonObject.Parse(body);
                JsonArray array = json.GetNamedArray("playlists");
                int count = array.Count;
                for (int i = 0; i < count; i++)
                {
                    JsonObject json1 = array.GetObjectAt((uint)i);
                    string id = json1.GetNamedString("id");
                    int itemcount = (int)json1.GetNamedNumber("itemCount");
                    url = Server_addr + "/api/playlists/" + id + "/items/0:" + itemcount.ToString() + "?columns=%title%,%artist%,%album%,%length%";
                    response = await http.GetAsync(new Uri(url));
                    response.EnsureSuccessStatusCode();
                    body = await response.Content.ReadAsStringAsync();
                    JsonObject json2 = JsonObject.Parse(body);
                    json2 = json2.GetNamedObject("playlistItems");
                    JsonArray array2 = json2.GetNamedArray("items");
                    for (int j = 0; j < itemcount; j++)
                    {
                        JsonObject column = array2.GetObjectAt((uint)j);
                        JsonArray columns = column.GetNamedArray("columns");
                        db_insert_song(
                            Server_id,
                            id,
                            j,
                            columns.GetStringAt(0),
                            columns.GetStringAt(1),
                            columns.GetStringAt(2),
                            columns.GetStringAt(3)
                            );
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                print(ex.HResult.ToString());
                string sql = "delete from list_s" + Server_id.ToString();
                ISQLiteStatement stat = db.Prepare(sql);
                stat.Step();
                stat.Dispose();
                return false;
            }
            
            
        }
        public static void db_insert_song(int server, string list, int id, string title, string artist, string album, string length)
        {
            title = title.Replace("'", "''");
            artist = artist.Replace("'", "''");
            album = album.Replace("'", "''");
            string sql = "insert into list_s" + server.ToString() + "(list,id,title,artist,album,length) values('" + list + "'," + id.ToString() + ",'" + title + "','" + artist + "','" + album + "','" + length + "')";
            ISQLiteStatement stat = db.Prepare(sql);
            stat.Step();
            stat.Dispose();
        }

        public static void db_set_current_playlist(string list)
        {
            string sql = "update current set playlist='" + list + "'";
            ISQLiteStatement stat = db.Prepare(sql);
            stat.Step();
            stat.Dispose();
        }

        public async static Task<JsonArray> db_get_tracks_by_list(string list, string search, int order)
        {
            JsonArray array = new JsonArray();
            string order_sql;
            switch (order)
            {
                case ORDER_DEFAULT:
                    order_sql = "";
                    break;
                case ORDER_TITLE_ASC:
                    order_sql = "order by title";
                    break;
                case ORDER_TITLE_DESC:
                    order_sql = "order by title desc";
                    break;
                case ORDER_ARTIST_ASC:
                    order_sql = "order by artist";
                    break;
                case ORDER_ARTIST_DESC:
                    order_sql = "order by artist desc";
                    break;
                case ORDER_ALBUM_ASC:
                    order_sql = "order by album";
                    break;
                case ORDER_ALBUM_DESC:
                    order_sql = "order by album desc";
                    break;
                case ORDER_RANDOM:
                    order_sql = "order by random()";
                    break;
                default:
                    order_sql = "";
                    break;
            }

            if (search.Equals(""))
            {
                string sql = "select id,title,artist,album,length from list_s" + Server_id.ToString() + " where list='" + list + "' " + order_sql + " limit " + tracks_limit.ToString();
                ISQLiteStatement stat = db.Prepare(sql);
                SQLiteResult result = stat.Step();
                while (SQLiteResult.ROW == result)
                {
                    JsonObject json = new JsonObject();
                    json.SetNamedValue("id", JsonValue.CreateStringValue(stat.GetInteger(0).ToString()));
                    json.SetNamedValue("title", JsonValue.CreateStringValue(stat[1] as string));
                    json.SetNamedValue("artist", JsonValue.CreateStringValue(stat[2] as string));
                    json.SetNamedValue("album", JsonValue.CreateStringValue(stat[3] as string));
                    json.SetNamedValue("length", JsonValue.CreateStringValue(stat[4] as string));
                    array.Add(json);
                    result = stat.Step();
                }
                stat.Dispose();
            }
            else
            {
                string[] searches = search.Split(" ");
                SortedSet<int> set = new SortedSet<int>();
                set.Clear();
                int count = 0;
                string sql = "select count(*) from list_s" + Server_id.ToString() + " where list='" + list + "'";
                ISQLiteStatement stat = db.Prepare(sql);
                SQLiteResult result = stat.Step();
                if (SQLiteResult.ROW == result)
                {
                    count = (int)stat.GetInteger(0);
                }
                stat.Dispose();
                for (int i = 0; i < count; i++)
                {
                    set.Add(i);
                }
                System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(@"^\d+$");
                foreach (string sea in searches)
                {
                    SortedSet<int> set1 = new SortedSet<int>();
                    set1.Clear();
                    if (accurate_search ? rex.IsMatch(search) : rex.IsMatch(sea))
                    {
                        sql = "select id from list_s" + Server_id.ToString() + " where list='" + list + "' and id='" + Int32.Parse(accurate_search ? search : sea).ToString() + "'";
                        stat = db.Prepare(sql);
                        result = stat.Step();
                        while (SQLiteResult.ROW == result)
                        {
                            int id = (int)stat.GetInteger(0);
                            set1.Add(id);
                            result = stat.Step();
                        }
                        stat.Dispose();
                    }

                    if (!accurate_search)
                    {
                        string condition = "%" + sqliteEscape(sea) + "%";
                        sql = "select id from list_s" + Server_id + " where list='" + list + "' and (title like '" + condition + "' escape '\\' or artist like '" + condition + "' escape '\\' or album like '" + condition + "' escape '\\')";
                    }
                    else
                    {
                        string condition = sqliteEscape(search);
                        sql = "select id from list_s" + Server_id + " where list='" + list + "' and (title='" + condition + "' collate nocase or artist='" + condition + "' collate nocase or album='" + condition + "' collate nocase)";
                    }
                    stat = db.Prepare(sql);
                    result = stat.Step();
                    while (SQLiteResult.ROW == result)
                    {
                        int id = (int)stat.GetInteger(0);
                        set1.Add(id);
                        result = stat.Step();
                    }
                    stat.Dispose();
                    set.IntersectWith(set1);
                    if (accurate_search)
                    {
                        break;
                    }

                }

                int set_size = set.Count;
                IEnumerator<int> enumerator =  set.GetEnumerator();
                string ids = "(";
                for (int i = 0; i < set_size; i++)
                {
                    if (i != 0) ids += ",";
                    enumerator.MoveNext();
                    ids += enumerator.Current.ToString();

                }
                ids += ")";

                sql = "select id,title,artist,album,length from list_s" + Server_id.ToString() + " where list='" + list + "' and id in " + ids + " limit " + tracks_limit.ToString();
                stat = db.Prepare(sql);
                result = stat.Step();
                while (SQLiteResult.ROW == result)
                {
                    JsonObject json = new JsonObject();
                    json.SetNamedValue("id", JsonValue.CreateStringValue(stat.GetInteger(0).ToString()));
                    json.SetNamedValue("title", JsonValue.CreateStringValue(stat[1] as string));
                    json.SetNamedValue("artist", JsonValue.CreateStringValue(stat[2] as string));
                    json.SetNamedValue("album", JsonValue.CreateStringValue(stat[3] as string));
                    json.SetNamedValue("length", JsonValue.CreateStringValue(stat[4] as string));
                    array.Add(json);
                    result = stat.Step();
                }
                stat.Dispose();
            }


            return array;
        }

        public static string sqliteEscape(string keyWord)
        {
            keyWord = keyWord.Replace("\\", "\\\\");
            keyWord = keyWord.Replace("%", "\\%");
            keyWord = keyWord.Replace("_", "\\_");
            keyWord = keyWord.Replace("'", "''");
            return keyWord;
        }
    }
}
